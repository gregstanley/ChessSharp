using ChessSharp.Enums;
using ChessSharp.Extensions;
using ChessSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace ChessSharp.MoveGeneration
{
    /*
     * Using ideas from Pradyumna Kannan.
     */
    public class MoveGenerator
    {
        private static readonly AttackBitmaps AttackBitmaps = new AttackBitmaps();

        private static readonly Vector<ulong> VectorZero = new Vector<ulong>(0);

        private MoveGenerationWorkspace[] workspaces;

        public MoveGenerator()
        {
            workspaces = new MoveGenerationWorkspace[] { new MoveGenerationWorkspace(0) };
        }

        public MoveGenerator(int workspaceCount)
        {
            workspaces = new MoveGenerationWorkspace[workspaceCount];

            for (var i = 0; i < workspaceCount; ++i)
                workspaces[i] = new MoveGenerationWorkspace(i);
        }

        public void Generate(Board board, Colour colour, IList<uint> moves)
        {
            foreach (var move in GenerateStream(0, board, colour))
                moves.Add(move);
        }

        public void Generate(ushort depth, Board board, Colour colour, IList<uint> moves)
        {
            if (depth >= workspaces.Length)
                throw new ArgumentException($"No buffers available for depth {depth}", nameof(depth));

            foreach (var move in GenerateStream(depth, board, colour))
                moves.Add(move);
        }

        public IEnumerable<uint> GenerateStream(ushort depth, Board board, Colour colour)
        {
            if (depth >= workspaces.Length)
                throw new ArgumentException($"No buffers available for depth {depth}", nameof(depth));

            var workspace = workspaces[depth];

            var relativeBitBoard = workspace.Reset(board, colour);

            var kingSquare = relativeBitBoard.MyKing.ToSquare();

            var kingRayAttackSquaresRook = GetAttackableSquares(kingSquare, PieceType.Rook, relativeBitBoard.OccupiedSquares);
            var kingRayAttackSquaresBishop = GetAttackableSquares(kingSquare, PieceType.Bishop, relativeBitBoard.OccupiedSquares);
            var kingRayAttackSquares = kingRayAttackSquaresRook | kingRayAttackSquaresBishop;

            var checkersPawn = relativeBitBoard.Colour == Colour.White
                ? AttackBitmaps.PawnCapturesWhite[kingSquare.Index] & relativeBitBoard.OpponentPawns
                : AttackBitmaps.PawnCapturesBlack[kingSquare.Index] & relativeBitBoard.OpponentPawns;

            var checkersKnight = (SquareFlag)0;
            var checkersRook = (SquareFlag)0;
            var checkersBishop = (SquareFlag)0;
            var checkersQueen = (SquareFlag)0;

            var buffer1 = workspace.Buffer1;
            var buffer2 = workspace.Buffer2;

            var anyNonPawnCheckers = false;

            if (Vector<ulong>.Count == 4)
            {
                buffer1[0] = (ulong)AttackBitmaps.KnightAttacks[kingSquare.Index];
                buffer1[1] = (ulong)kingRayAttackSquaresRook;
                buffer1[2] = (ulong)kingRayAttackSquaresBishop;
                buffer1[3] = (ulong)kingRayAttackSquares;

                buffer2[0] = (ulong)relativeBitBoard.OpponentKnights;
                buffer2[1] = (ulong)relativeBitBoard.OpponentRooks;
                buffer2[2] = (ulong)relativeBitBoard.OpponentBishops;
                buffer2[3] = (ulong)relativeBitBoard.OpponentQueens;

                var vector1 = new Vector<ulong>(buffer1);
                var vector2 = new Vector<ulong>(buffer2);

                var vectorOut = Vector.BitwiseAnd(vector1, vector2);

                anyNonPawnCheckers = Vector.GreaterThanAny(vectorOut, VectorZero);

                checkersKnight = (SquareFlag)vectorOut[0];
                checkersRook = (SquareFlag)vectorOut[1];
                checkersBishop = (SquareFlag)vectorOut[2];
                checkersQueen = (SquareFlag)vectorOut[3];
            }
            else
            {
                checkersKnight = AttackBitmaps.KnightAttacks[kingSquare.Index] & relativeBitBoard.OpponentKnights;

                checkersRook = relativeBitBoard.OpponentRooks == 0 ? 0
                    : kingRayAttackSquaresRook & relativeBitBoard.OpponentRooks;

                checkersBishop = relativeBitBoard.OpponentBishops == 0 ? 0
                    : kingRayAttackSquaresBishop & relativeBitBoard.OpponentBishops;

                checkersQueen = relativeBitBoard.OpponentQueens == 0 ? 0
                    : kingRayAttackSquares & relativeBitBoard.OpponentQueens;

                if (checkersKnight > 0 || checkersRook > 0 || checkersBishop > 0 || checkersQueen > 0)
                    anyNonPawnCheckers = true;
            }

            // Start on King moves - if we're in Check then that might be all we need
            var attackableSquares = AttackBitmaps.KingAttacks[kingSquare.Index] & ~relativeBitBoard.MySquares;

            var unsafeSquares = (SquareFlag)0;

            var checkers = (SquareFlag)0;

            if (checkersPawn > 0 || anyNonPawnCheckers)
            {
                // As we have information on Checks use that to find dangerous squares
                if (checkersRook > 0)
                    unsafeSquares |= AttackBitmaps.Paths[kingSquare.Index][checkersRook.ToSquareIndex()] & ~checkersRook;

                if (checkersBishop > 0)
                    unsafeSquares |= AttackBitmaps.Paths[kingSquare.Index][checkersBishop.ToSquareIndex()] & ~checkersBishop;

                if (checkersQueen > 0)
                    unsafeSquares |= AttackBitmaps.Paths[kingSquare.Index][checkersQueen.ToSquareIndex()] & ~checkersQueen;

                checkers = checkersPawn | checkersKnight | checkersRook | checkersBishop | checkersQueen;
            }

            var semiSafeAttackableSquares = attackableSquares & ~unsafeSquares;

            var semiSafeAttackableSquaresAsList = semiSafeAttackableSquares.ToList();

            var safeSquaresAsList = FindSafeSquares(workspace, semiSafeAttackableSquaresAsList);

            workspace.NumCheckers = checkers.Count();

            foreach (var toSquare in safeSquaresAsList)
            {
                if (relativeBitBoard.OpponentSquares.HasFlag(toSquare))
                {
                    var capturePieceType = relativeBitBoard.GetPieceType(toSquare);

                    var kingMove = MoveBuilder.Create(relativeBitBoard.Colour, PieceType.King, kingSquare, toSquare.ToSquare(), capturePieceType, MoveType.Ordinary, workspace.NumCheckers);

                    workspace.KingCaptureMoveBuffer.Add(kingMove);
                }
                else
                {
                    var kingMove = MoveBuilder.Create(relativeBitBoard.Colour, PieceType.King, kingSquare, toSquare.ToSquare(), PieceType.None, MoveType.Ordinary, workspace.NumCheckers);

                    workspace.KingNonCaptureMoveBuffer.Add(kingMove);
                }
            }

            // If in Check by more than one piece then only options are King moves - which we already have
            if (workspace.NumCheckers > 1)
            {
                foreach (var move in workspace.KingCaptureMoveBuffer)
                    yield return move;

                workspace.KingCaptureMoveBuffer.Clear();

                foreach (var move in workspace.KingNonCaptureMoveBuffer)
                    yield return move;

                workspace.KingNonCaptureMoveBuffer.Clear();

                yield break;
            }
            
            // https://peterellisjones.com/posts/generating-legal-chess-moves-efficiently/
            // By default - whole board. If in Check it becomes limited to the Checker (no other captures count)
            var captureMask = SquareFlagConstants.All;

            // By default - whole board. If in Check by ray piece it becomes limited to popsitions between King and Checker
            var pushMask = SquareFlagConstants.All;

            if (workspace.NumCheckers == 1)
            {
                captureMask = checkers;

                var rayChecker = checkers & relativeBitBoard.OpponentRaySquares;

                if (rayChecker > 0)
                {
                    var checkerSquareIndex = rayChecker.ToSquareIndex();

                    var pathFromCheckerToKing = AttackBitmaps.Paths[checkerSquareIndex][kingSquare.Index];

                    // In Check by ray piece so non capture moves must end on this line
                    pushMask = pathFromCheckerToKing & ~kingSquare.Flag & ~rayChecker;
                }
                else
                {
                    // Not a ray piece so force captures only
                    pushMask = 0;
                }
            }
            else
            {
                // Only run castle logic if we are not in check
                AddCastles(workspace, kingSquare);
            }

            var kingRayAttackSquaresWithoutKing = kingRayAttackSquares & ~kingSquare.Flag;

            // Pass the relative bitboard in separately to avoid it being re-generated
            var pinnedSquares = AddPinnedMoves(workspace, kingSquare, kingRayAttackSquaresWithoutKing, pushMask, captureMask);

            foreach (var move in workspace.CaptureMoveBuffer)
                yield return move;

            workspace.CaptureMoveBuffer.Clear();

            var legalMask = pushMask | captureMask;

            var remainingSquares = ~pinnedSquares;

            var remainingPawns = (SquareFlag)0;
            var remainingKnights = (SquareFlag)0;
            var remainingRooks = (SquareFlag)0;
            var remainingBishops = (SquareFlag)0;

            if (Vector<ulong>.Count == 4)
            {
                buffer1[0] = (ulong)relativeBitBoard.MyPawns;
                buffer1[1] = (ulong)relativeBitBoard.MyKnights;
                buffer1[2] = (ulong)relativeBitBoard.MyRooks;
                buffer1[3] = (ulong)relativeBitBoard.MyBishops;

                buffer2[0] = (ulong)remainingSquares;
                buffer2[1] = (ulong)remainingSquares;
                buffer2[2] = (ulong)remainingSquares;
                buffer2[3] = (ulong)remainingSquares;

                var vector1 = new Vector<ulong>(buffer1);
                var vector2 = new Vector<ulong>(buffer2);

                var vectorOut = Vector.BitwiseAnd(vector1, vector2);

                remainingPawns = (SquareFlag)vectorOut[0];
                remainingKnights = (SquareFlag)vectorOut[1];
                remainingRooks = (SquareFlag)vectorOut[2];
                remainingBishops = (SquareFlag)vectorOut[3];
            }
            else
            {
                remainingPawns = relativeBitBoard.MyPawns & remainingSquares;
                remainingKnights = relativeBitBoard.MyKnights & remainingSquares;
                remainingRooks = relativeBitBoard.MyRooks & remainingSquares;
                remainingBishops = relativeBitBoard.MyBishops & remainingSquares;
            }

            if (relativeBitBoard.MyPawns > 0)
            {
                var singlePushes = remainingPawns.PawnForward(relativeBitBoard.Colour, 1);

                var unblockedSinglePushes = singlePushes & ~relativeBitBoard.OccupiedSquares;
                var unblockedAndLegalSinglePushes = unblockedSinglePushes & pushMask;

                var promotionPushes = relativeBitBoard.PromotionRank & unblockedAndLegalSinglePushes;
                var nonPromotionPushes = ~relativeBitBoard.PromotionRank & unblockedAndLegalSinglePushes;
                var firstPushes = unblockedSinglePushes & relativeBitBoard.SecondRank;

                var potentialWestCaptureMask = remainingPawns.PawnCaptureWest(relativeBitBoard.Colour);
                var potentialEastCaptureMask = remainingPawns.PawnCaptureEast(relativeBitBoard.Colour);

                var potentialWestCaptures = potentialWestCaptureMask & relativeBitBoard.OpponentSquares;
                var potentialEastCaptures = potentialEastCaptureMask & relativeBitBoard.OpponentSquares;

                var potentialPiecesWest = potentialWestCaptures.PawnCaptureEast(relativeBitBoard.OpponentColour);
                var potentialPiecesEast = potentialEastCaptures.PawnCaptureWest(relativeBitBoard.OpponentColour);

                var potentialCapturePieces = potentialPiecesWest | potentialPiecesEast;

                // Very basic but if En Passant is set then check any pawn on the row
                if (relativeBitBoard.EnPassant > 0)
                {
                    var remainingPotentialEnPassant = remainingPawns & relativeBitBoard.EnPassantDiscoveredCheckRank;

                    potentialCapturePieces |= remainingPotentialEnPassant;
                }

                if (potentialCapturePieces > 0)
                {
                    foreach (var pieceSquare in potentialCapturePieces.ToList())
                        AddIndividualPawnCaptures(workspace, pieceSquare.ToSquare(), pushMask, captureMask);
                }

                foreach (var move in workspace.CaptureMoveBuffer)
                    yield return move;

                workspace.CaptureMoveBuffer.Clear();

                if (promotionPushes > 0)
                {
                    foreach (var pieceSquare in promotionPushes.ToList())
                    {
                        var fromSquare = pieceSquare.PawnBackward(relativeBitBoard.Colour, 1);

                        AddPromotions(workspace, fromSquare.ToSquare(), pieceSquare.ToSquare(), PieceType.None);
                    }
                }

                if (nonPromotionPushes > 0)
                {
                    foreach (var pieceSquare in nonPromotionPushes.ToList())
                    {
                        var fromSquare = pieceSquare.PawnBackward(relativeBitBoard.Colour, 1);

                        workspace.NonCaptureMoveBuffer.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.Pawn, fromSquare.ToSquare(), pieceSquare.ToSquare(), PieceType.None, MoveType.Ordinary, workspace.NumCheckers));
                    }
                }

                if (firstPushes > 0)
                {
                    var doublePushes = firstPushes.PawnForward(relativeBitBoard.Colour, 1);

                    var validDoublePushes = doublePushes & ~relativeBitBoard.OccupiedSquares & pushMask;

                    if (validDoublePushes > 0)
                    {
                        foreach (var pieceSquare in validDoublePushes.ToList())
                        {
                            var fromSquare = pieceSquare.PawnBackward(relativeBitBoard.Colour, 2);

                            workspace.NonCaptureMoveBuffer.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.Pawn, fromSquare.ToSquare(), pieceSquare.ToSquare(), PieceType.None, MoveType.Ordinary, workspace.NumCheckers));
                        }
                    }
                }
            }

            if (relativeBitBoard.MyKnights > 0)
            {
                foreach (var pieceSquareFlag in remainingKnights.ToList())
                    AddIndividualKnightMoves(workspace, relativeBitBoard, pieceSquareFlag.ToSquare(), legalMask);
            }

            foreach (var move in workspace.CaptureMoveBuffer)
                yield return move;

            workspace.CaptureMoveBuffer.Clear();

            if (relativeBitBoard.MyRooks > 0)
            {
                foreach (var pieceSquare in remainingRooks.ToList())
                    AddIndividualRayMoves(workspace, pieceSquare.ToSquare(), PieceType.Rook, PieceType.Rook, legalMask);
            }

            foreach (var move in workspace.CaptureMoveBuffer)
                yield return move;

            workspace.CaptureMoveBuffer.Clear();

            if (relativeBitBoard.MyBishops > 0)
            {
                foreach (var pieceSquare in remainingBishops.ToList())
                    AddIndividualRayMoves(workspace, pieceSquare.ToSquare(), PieceType.Bishop, PieceType.Bishop, legalMask);
            }

            foreach (var move in workspace.CaptureMoveBuffer)
                yield return move;

            workspace.CaptureMoveBuffer.Clear();

            if (relativeBitBoard.MyQueens > 0)
            {
                var remainingQueens = relativeBitBoard.MyQueens & remainingSquares;

                foreach (var pieceSquare in remainingQueens.ToList())
                    AddIndividualQueenMoves(workspace, pieceSquare.ToSquare(), legalMask);
            }

            foreach (var move in workspace.CaptureMoveBuffer)
                yield return move;

            workspace.CaptureMoveBuffer.Clear();

            foreach (var move in workspace.KingCaptureMoveBuffer)
                yield return move;

            workspace.KingCaptureMoveBuffer.Clear();

            // Finally, clear out all the non-capture moves
            foreach (var move in workspace.NonCaptureMoveBuffer)
                yield return move;

            workspace.NonCaptureMoveBuffer.Clear();

            foreach (var move in workspace.KingNonCaptureMoveBuffer)
                yield return move;

            workspace.KingNonCaptureMoveBuffer.Clear();
        }

        private IList<SquareFlag> FindSafeSquares(MoveGenerationWorkspace workspace, IEnumerable<SquareFlag> attackableSquares)
        {
            var relativeBitBoard = workspace.RelativeBitBoard;

            var safeSquares = workspace.SafeSquares;

            safeSquares.Clear();

            var unsafeSquares = (SquareFlag)0;

            var buffer1 = workspace.Buffer1;
            var buffer2 = workspace.Buffer2;

            foreach (var attackableSquare in attackableSquares)
            {
                if (unsafeSquares.HasFlag(attackableSquare))
                    continue;

                var targetSquare = attackableSquare.ToSquare();

                var opponentPawnAttacks = relativeBitBoard.Colour == Colour.White
                    ? AttackBitmaps.PawnCapturesWhite[targetSquare.Index]
                    : AttackBitmaps.PawnCapturesBlack[targetSquare.Index];

                var potentialCheckersPawn = (SquareFlag)0;
                var potentialCheckersKing = (SquareFlag)0;
                var potentialCheckersKnight = (SquareFlag)0;
                var occupiedSquaresWithoutMyKing = (SquareFlag)0;

                if (Vector<ulong>.Count == 4)
                {
                    buffer1[0] = (ulong)opponentPawnAttacks;
                    buffer1[1] = (ulong)AttackBitmaps.KingAttacks[targetSquare.Index];
                    buffer1[2] = (ulong)AttackBitmaps.KnightAttacks[targetSquare.Index];
                    buffer1[3] = (ulong)relativeBitBoard.OccupiedSquares;

                    buffer2[0] = (ulong)relativeBitBoard.OpponentPawns;
                    buffer2[1] = (ulong)relativeBitBoard.OpponentKing;
                    buffer2[2] = (ulong)relativeBitBoard.OpponentKnights;
                    buffer2[3] = (ulong)~relativeBitBoard.MyKing;

                    var vector1 = new Vector<ulong>(buffer1);
                    var vector2 = new Vector<ulong>(buffer2);

                    var vectorOut = Vector.BitwiseAnd(vector1, vector2);

                    potentialCheckersPawn = (SquareFlag)vectorOut[0];
                    potentialCheckersKing = (SquareFlag)vectorOut[1];
                    potentialCheckersKnight = (SquareFlag)vectorOut[2];
                    occupiedSquaresWithoutMyKing = (SquareFlag)vectorOut[3];
                }
                else
                {
                    potentialCheckersPawn = opponentPawnAttacks & relativeBitBoard.OpponentPawns;
                    potentialCheckersKing = AttackBitmaps.KingAttacks[targetSquare.Index] & relativeBitBoard.OpponentKing;
                    potentialCheckersKnight = AttackBitmaps.KnightAttacks[targetSquare.Index] & relativeBitBoard.OpponentKnights;
                    occupiedSquaresWithoutMyKing = relativeBitBoard.OccupiedSquares & ~relativeBitBoard.MyKing;
                }

                if (potentialCheckersKing > 0)
                    continue;

                if (potentialCheckersPawn > 0)
                    continue;

                if (potentialCheckersKnight > 0)
                    continue;

                var attackableRookSquares = GetAttackableSquares(targetSquare, PieceType.Rook, occupiedSquaresWithoutMyKing);
                var attackableBishopSquares = GetAttackableSquares(targetSquare, PieceType.Bishop, occupiedSquaresWithoutMyKing);

                var potentialCheckersRook = (SquareFlag)0;
                var potentialCheckersBishop = (SquareFlag)0;
                var potentialCheckersQueenAsRook = (SquareFlag)0;
                var potentialCheckersQueenAsBishop = (SquareFlag)0;

                if (Vector<ulong>.Count == 4)
                {
                    buffer1[0] = (ulong)attackableRookSquares;
                    buffer1[1] = (ulong)attackableBishopSquares;
                    buffer1[2] = (ulong)attackableRookSquares;
                    buffer1[3] = (ulong)attackableBishopSquares;

                    buffer2[0] = (ulong)relativeBitBoard.OpponentRooks;
                    buffer2[1] = (ulong)relativeBitBoard.OpponentBishops;
                    buffer2[2] = (ulong)relativeBitBoard.OpponentQueens;
                    buffer2[3] = (ulong)relativeBitBoard.OpponentQueens;

                    var vector1 = new Vector<ulong>(buffer1);
                    var vector2 = new Vector<ulong>(buffer2);

                    var vectorOut = Vector.BitwiseAnd(vector1, vector2);

                    potentialCheckersRook = (SquareFlag)vectorOut[0];
                    potentialCheckersBishop = (SquareFlag)vectorOut[1];
                    potentialCheckersQueenAsRook = (SquareFlag)vectorOut[2];
                    potentialCheckersQueenAsBishop = (SquareFlag)vectorOut[3];
                }
                else
                {
                    potentialCheckersRook = attackableRookSquares & relativeBitBoard.OpponentRooks;
                    potentialCheckersBishop = attackableBishopSquares & relativeBitBoard.OpponentBishops;
                    potentialCheckersQueenAsRook = attackableRookSquares & relativeBitBoard.OpponentQueens;
                    potentialCheckersQueenAsBishop = attackableBishopSquares & relativeBitBoard.OpponentQueens;
                }

                if (potentialCheckersRook > 0)
                {
                    foreach (var potentialCheckerRook in potentialCheckersRook.ToList())
                        unsafeSquares |= AttackBitmaps.Paths[targetSquare.Index][potentialCheckerRook.ToSquareIndex()] & ~potentialCheckerRook;

                    continue;
                }

                if (potentialCheckersBishop > 0)
                {
                    foreach (var potentialCheckerBishop in potentialCheckersBishop.ToList())
                        unsafeSquares |= AttackBitmaps.Paths[targetSquare.Index][potentialCheckerBishop.ToSquareIndex()] & ~potentialCheckerBishop;

                    continue;
                }

                if (potentialCheckersQueenAsRook > 0)
                {
                    foreach (var potentialCheckerQueenAsRook in potentialCheckersQueenAsRook.ToList())
                        unsafeSquares |= AttackBitmaps.Paths[targetSquare.Index][potentialCheckerQueenAsRook.ToSquareIndex()] & ~potentialCheckerQueenAsRook;

                    continue;
                }

                if (potentialCheckersQueenAsBishop > 0)
                {
                    foreach (var potentialCheckerQueenAsBishop in potentialCheckersQueenAsBishop.ToList())
                        unsafeSquares |= AttackBitmaps.Paths[targetSquare.Index][potentialCheckerQueenAsBishop.ToSquareIndex()] & ~potentialCheckerQueenAsBishop;

                    continue;
                }

                safeSquares.Add(attackableSquare);
            }

            return safeSquares;
        }

        private SquareFlag AddPinnedMoves(
            MoveGenerationWorkspace workspace,
            Square kingSquare,
            SquareFlag kingRayAttackSquares,
            SquareFlag pushMask,
            SquareFlag captureMask)
        {
            var relativeBitBoard = workspace.RelativeBitBoard;

            var potentialPins = kingRayAttackSquares & relativeBitBoard.MySquares;

            var occupiedSquaresWithoutPotentialPins = relativeBitBoard.OccupiedSquares & ~potentialPins;

            var attackableSquaresBeyondPinsRook = GetAttackableSquares(kingSquare, PieceType.Rook, occupiedSquaresWithoutPotentialPins);
            var attackableSquaresBeyondPinsBishop = GetAttackableSquares(kingSquare, PieceType.Bishop, occupiedSquaresWithoutPotentialPins);
            var attackableSquaresBeyondPins = attackableSquaresBeyondPinsRook | attackableSquaresBeyondPinsBishop;

            var pinningRooks = (SquareFlag)0;
            var pinningBishops = (SquareFlag)0;
            var pinningQueensNonDiagonal = (SquareFlag)0;
            var pinningQueensDiagonal = (SquareFlag)0;

            var buffer1 = workspace.Buffer1;
            var buffer2 = workspace.Buffer2;

            if (Vector<ulong>.Count == 4)
            {
                buffer1[0] = (ulong)attackableSquaresBeyondPinsRook;
                buffer1[1] = (ulong)attackableSquaresBeyondPinsBishop;
                buffer1[2] = (ulong)attackableSquaresBeyondPinsRook;
                buffer1[3] = (ulong)attackableSquaresBeyondPinsBishop;

                buffer2[0] = (ulong)relativeBitBoard.OpponentRooks;
                buffer2[1] = (ulong)relativeBitBoard.OpponentBishops;
                buffer2[2] = (ulong)relativeBitBoard.OpponentQueens;
                buffer2[3] = (ulong)relativeBitBoard.OpponentQueens;

                var vector1 = new Vector<ulong>(buffer1);
                var vector2 = new Vector<ulong>(buffer2);

                var vectorOut = Vector.BitwiseAnd(vector1, vector2);

                var anyGreaterThanZero = Vector.GreaterThanAny(vectorOut, new Vector<ulong>(0));

                if (!anyGreaterThanZero)
                    return 0;

                pinningRooks = (SquareFlag)vectorOut[0];
                pinningBishops = (SquareFlag)vectorOut[1];
                pinningQueensNonDiagonal = (SquareFlag)vectorOut[2];
                pinningQueensDiagonal = (SquareFlag)vectorOut[3];
            }
            else
            {
                pinningRooks = attackableSquaresBeyondPinsRook & relativeBitBoard.OpponentRooks;
                pinningBishops = attackableSquaresBeyondPinsBishop & relativeBitBoard.OpponentBishops;
                pinningQueensNonDiagonal = attackableSquaresBeyondPinsRook & relativeBitBoard.OpponentQueens;
                pinningQueensDiagonal = attackableSquaresBeyondPinsBishop & relativeBitBoard.OpponentQueens;
            }

            var pinnedPieces = (SquareFlag)0;

            if (pinningRooks > 0)
                pinnedPieces |= AddPinnedMovesInternal(workspace, kingSquare, potentialPins, pinningRooks, false, pushMask, captureMask);

            if (pinningBishops > 0)
                pinnedPieces |= AddPinnedMovesInternal(workspace, kingSquare, potentialPins, pinningBishops, true, pushMask, captureMask);

            if (pinningQueensNonDiagonal > 0)
                pinnedPieces |= AddPinnedMovesInternal(workspace, kingSquare, potentialPins, pinningQueensNonDiagonal, false, pushMask, captureMask);

            if (pinningQueensDiagonal > 0)
                pinnedPieces |= AddPinnedMovesInternal(workspace, kingSquare, potentialPins, pinningQueensDiagonal, true, pushMask, captureMask);

            return pinnedPieces;
        }

        private SquareFlag AddPinnedMovesInternal(
            MoveGenerationWorkspace workspace,
            Square kingSquare,
            SquareFlag potentialPins,
            SquareFlag pinners,
            bool diagonal,
            SquareFlag pushMask,
            SquareFlag captureMask)
        {
            var pinnedSquares = (SquareFlag)0;
            var pinnersAsList = pinners.ToList();

            foreach (var pinner in pinnersAsList)
            {
                var path = AttackBitmaps.Paths[kingSquare.Index][pinner.ToSquareIndex()];

                var squarePinnedByThisPiece = path & potentialPins;

                if (squarePinnedByThisPiece == 0)
                    continue;

                pinnedSquares |= squarePinnedByThisPiece;

                var pushPath = path & pushMask;
                var capturePath = path & captureMask;

                var pieceType = workspace.RelativeBitBoard.GetPieceType(squarePinnedByThisPiece);

                switch (pieceType)
                {
                    case PieceType.Pawn when !diagonal:
                        AddIndividualPawnPushes(workspace, squarePinnedByThisPiece.ToSquare(), pushPath);
                        break;
                    case PieceType.Pawn when diagonal:
                        AddIndividualPawnCaptures(workspace, squarePinnedByThisPiece.ToSquare(), pushPath, capturePath);
                        break;
                    case PieceType.Rook:
                        AddIndividualRayMoves(workspace, squarePinnedByThisPiece.ToSquare(), PieceType.Rook, PieceType.Rook, pushPath | capturePath);
                        break;
                    case PieceType.Bishop:
                        AddIndividualRayMoves(workspace, squarePinnedByThisPiece.ToSquare(), PieceType.Bishop, PieceType.Bishop, pushPath | capturePath);
                        break;
                    case PieceType.Queen:
                        AddIndividualQueenMoves(workspace, squarePinnedByThisPiece.ToSquare(), pushPath | capturePath);
                        break;
                }
            }

            return pinnedSquares;
        }
        
        private void AddIndividualPawnPushes(MoveGenerationWorkspace workspace, Square fromSquare, SquareFlag pushMask)
        {
            var relativeBitBoard = workspace.RelativeBitBoard;

            var toSquare = fromSquare.Flag
                .PawnForward(relativeBitBoard.Colour, 1)
                .ToSquare();

            if (relativeBitBoard.OccupiedSquares.HasFlag(toSquare.Flag))
                return;

            if (pushMask.HasFlag(toSquare.Flag))
            {
                if (relativeBitBoard.PromotionRank.HasFlag(toSquare.Flag))
                    AddPromotions(workspace, fromSquare, toSquare, PieceType.None);
                else
                    workspace.NonCaptureMoveBuffer.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, PieceType.None, MoveType.Ordinary, workspace.NumCheckers));
            }

            // Possible that we can block check with double push
            if (relativeBitBoard.StartRank.HasFlag(fromSquare.Flag))
            {
                toSquare = fromSquare.Flag
                    .PawnForward(relativeBitBoard.Colour, 2)
                    .ToSquare();

                if (!pushMask.HasFlag(toSquare.Flag))
                    return;

                // Promotions not possible from start rank
                if (!relativeBitBoard.OccupiedSquares.HasFlag(toSquare.Flag))
                    workspace.NonCaptureMoveBuffer.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, PieceType.None, MoveType.Ordinary, workspace.NumCheckers));
            }
        }

        private void AddIndividualPawnCaptures(MoveGenerationWorkspace workspace, Square fromSquare, SquareFlag pushMask, SquareFlag captureMask)
        {
            var relativeBitBoard = workspace.RelativeBitBoard;

            var captureSquares = relativeBitBoard.Colour == Colour.White
                ? AttackBitmaps.PawnCapturesWhite[fromSquare.Index].ToList()
                : AttackBitmaps.PawnCapturesBlack[fromSquare.Index].ToList();

            foreach (var toSquare in captureSquares)
            {
                var moveType = relativeBitBoard.EnPassant.HasFlag(toSquare) ? MoveType.EnPassant : MoveType.Ordinary;

                if (relativeBitBoard.OpponentSquares.HasFlag(toSquare) || moveType == MoveType.EnPassant)
                {
                    var capturePieceType = relativeBitBoard.GetPieceType(toSquare);

                    var discoveredCheck = false;

                    if (moveType != MoveType.EnPassant)
                    {
                        if (!captureMask.HasFlag(toSquare))
                            continue;
                    }
                    else
                    {
                        // Abuse the push system - push an imaginary pawn from the en passant square as opponent to find piece
                        var enPassantCaptureSquare = relativeBitBoard.EnPassant.PawnForward(relativeBitBoard.OpponentColour, 1);

                        var blockWithPush = pushMask.HasFlag(relativeBitBoard.EnPassant);

                        var evadeWithCapture = captureMask.HasFlag(enPassantCaptureSquare);

                        if (!blockWithPush && !evadeWithCapture)
                            continue;

                        capturePieceType = PieceType.Pawn;

                        // Looking for DISCOVERED CHECK (not spotted by pinned pieces as there are 2 pawns in the way)
                        // Slight duplication here but probably cleaner than passing in
                        var kingSquare = relativeBitBoard.MyKing.ToSquare();

                        var enPassantDiscoveredCheckRank = relativeBitBoard.EnPassantDiscoveredCheckRank;

                        // This should be super rare. Performing an en passant capture with our King on same rank.
                        if (enPassantDiscoveredCheckRank.HasFlag(kingSquare.Flag))
                        {
                            if ((enPassantDiscoveredCheckRank & relativeBitBoard.OpponentRooks) > 0
                                || (enPassantDiscoveredCheckRank & relativeBitBoard.OpponentQueens) > 0)
                            {
                                var occupancyMask = MagicNumbers.RookOccupancyMasks[kingSquare.Index];

                                var occupancyBeforeCapture = relativeBitBoard.OccupiedSquares & occupancyMask;

                                var occupancyAfterCapture = occupancyBeforeCapture
                                    & ~enPassantCaptureSquare
                                    & ~fromSquare.Flag;

                                // Search for magic moves using just the occupancy of rank (the rest is not relevant)
                                var magicIndex = MagicIndexHelpers.GetMagicIndex(PieceType.Rook, kingSquare.Index, occupancyAfterCapture);

                                var kingRayAttacks = AttackBitmaps.RookAttacks[kingSquare.Index][magicIndex];

                                var kingRayAttacksOnRank = kingRayAttacks & enPassantDiscoveredCheckRank;

                                discoveredCheck = (kingRayAttacksOnRank & relativeBitBoard.OpponentRooks) > 0
                                    || (kingRayAttacksOnRank & relativeBitBoard.OpponentQueens) > 0;
                            }
                        }
                    }

                    if (relativeBitBoard.PromotionRank.HasFlag(toSquare))
                    {
                        AddPromotions(workspace, fromSquare, toSquare.ToSquare(), capturePieceType);
                    }
                    else if (!discoveredCheck)
                    {
                        if (capturePieceType != PieceType.None)
                            workspace.CaptureMoveBuffer.Add(MoveBuilder.Create(workspace.Colour, PieceType.Pawn, fromSquare, toSquare.ToSquare(), capturePieceType, moveType, workspace.NumCheckers));
                        else
                            workspace.NonCaptureMoveBuffer.Add(MoveBuilder.Create(workspace.Colour, PieceType.Pawn, fromSquare, toSquare.ToSquare(), capturePieceType, moveType, workspace.NumCheckers));
                    }
                }
            }
        }

        private void AddIndividualKnightMoves(MoveGenerationWorkspace workspace, RelativeBoard relativeBitBoard, Square fromSquare, SquareFlag legalMask)
        {
            var attackableSquaresIncludingSelfCaptures = AttackBitmaps.KnightAttacks[fromSquare.Index];
            var attackableSquares = attackableSquaresIncludingSelfCaptures & ~relativeBitBoard.MySquares;
            var legalAttackableSquares = attackableSquares & legalMask;

            ToOrdinaryMoves(workspace, PieceType.Knight, fromSquare, legalAttackableSquares);
        }

        private void AddIndividualQueenMoves(MoveGenerationWorkspace workspace, Square fromSquare, SquareFlag legalMask)
        {
            AddIndividualRayMoves(workspace, fromSquare, PieceType.Rook, PieceType.Queen, legalMask);
            AddIndividualRayMoves(workspace, fromSquare, PieceType.Bishop, PieceType.Queen, legalMask);
        }

        private void AddIndividualRayMoves(MoveGenerationWorkspace workspace, Square fromSquare, PieceType rayType, PieceType pieceType, SquareFlag legalMask)
        {
            var attackableSquaresIncludingSelfCaptures = GetAttackableSquares(fromSquare, rayType, workspace.RelativeBitBoard.OccupiedSquares);
            var attackableSquares = attackableSquaresIncludingSelfCaptures & ~workspace.RelativeBitBoard.MySquares;
            var legalAttackableSquares = attackableSquares & legalMask;

            ToOrdinaryMoves(workspace, pieceType, fromSquare, legalAttackableSquares);
        }

        private void AddCastles(MoveGenerationWorkspace workspace, Square kingSquare)
        {
            var relativeBitBoard = workspace.RelativeBitBoard;

            if (!relativeBitBoard.CanCastleKingSide && !relativeBitBoard.CanCastleQueenSide)
                return;

            // We lose castle rights if any piece moves so they MUST be in correct locations
            if (relativeBitBoard.CanCastleKingSide)
            {
                var kingSideRookIndex = relativeBitBoard.KingSideRookStartSquare.ToSquareIndex();

                var kingToRook = AttackBitmaps.Paths[kingSquare.Index][kingSideRookIndex];

                var squaresBetween = kingToRook & ~relativeBitBoard.KingStartSquare & ~relativeBitBoard.KingSideRookStartSquare;

                if ((squaresBetween & relativeBitBoard.OccupiedSquares) == 0)
                {
                    var stepSquares = relativeBitBoard.KingSideCastleStep1 | relativeBitBoard.KingSideCastleStep2;
                    var safeSquaresAsList = FindSafeSquares(workspace, stepSquares.ToList());

                    var safeSquares = (SquareFlag)0;

                    foreach (var safeSquare in safeSquaresAsList)
                        safeSquares |= safeSquare;

                    if (squaresBetween == safeSquares)
                    {
                        workspace.NonCaptureMoveBuffer.Add(MoveBuilder.CreateCastle(relativeBitBoard.Colour, MoveType.CastleKing));
                    }
                }
            }

            if (relativeBitBoard.CanCastleQueenSide)
            {
                var queenSideRookIndex = relativeBitBoard.QueenSideRookStartSquare.ToSquareIndex();

                var kingToRook = AttackBitmaps.Paths[kingSquare.Index][queenSideRookIndex];

                var squaresBetween = kingToRook & ~relativeBitBoard.KingStartSquare & ~relativeBitBoard.QueenSideRookStartSquare;

                if ((squaresBetween & relativeBitBoard.OccupiedSquares) == 0)
                {
                    var stepSquares = relativeBitBoard.QueenSideCastleStep1 | relativeBitBoard.QueenSideCastleStep2;
                    var safeSquaresAsList = FindSafeSquares(workspace, stepSquares.ToList());

                    var safeSquares = (SquareFlag)0;

                    foreach (var safeSquare in safeSquaresAsList)
                        safeSquares |= safeSquare;

                    // On Queen side the King doesn't pass through B file so we don't look for Check there
                    var squaresBetweenMinusFirstRookStep = squaresBetween & ~relativeBitBoard.QueenSideRookStep1Square;

                    if (squaresBetweenMinusFirstRookStep == safeSquares)
                        workspace.NonCaptureMoveBuffer.Add(MoveBuilder.CreateCastle(relativeBitBoard.Colour, MoveType.CastleQueen));
                }
            }
        }

        private void AddPromotions(MoveGenerationWorkspace workspace, Square fromSquare, Square toSquare, PieceType capturePieceType)
        {
            if (capturePieceType != PieceType.None)
            {
                workspace.CaptureMoveBuffer.Add(MoveBuilder.Create(workspace.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionQueen, workspace.NumCheckers));
                workspace.CaptureMoveBuffer.Add(MoveBuilder.Create(workspace.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionRook, workspace.NumCheckers));
                workspace.CaptureMoveBuffer.Add(MoveBuilder.Create(workspace.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionKnight, workspace.NumCheckers));
                workspace.CaptureMoveBuffer.Add(MoveBuilder.Create(workspace.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionBishop, workspace.NumCheckers));
            }
            else
            {
                workspace.NonCaptureMoveBuffer.Add(MoveBuilder.Create(workspace.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionQueen, workspace.NumCheckers));
                workspace.NonCaptureMoveBuffer.Add(MoveBuilder.Create(workspace.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionRook, workspace.NumCheckers));
                workspace.NonCaptureMoveBuffer.Add(MoveBuilder.Create(workspace.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionKnight, workspace.NumCheckers));
                workspace.NonCaptureMoveBuffer.Add(MoveBuilder.Create(workspace.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionBishop, workspace.NumCheckers));
            }
        }

        private SquareFlag GetAttackableSquares(Square fromSquare, PieceType rayType, SquareFlag occupiedSquares)
        {
            var occupancyMask = rayType == PieceType.Rook
                ? MagicNumbers.RookOccupancyMasks[fromSquare.Index]
                : MagicNumbers.BishopOccupancyMasks[fromSquare.Index];

            var occupancyMasked = occupiedSquares & occupancyMask;

            var magicIndex = MagicIndexHelpers.GetMagicIndex(rayType, fromSquare.Index, occupancyMasked);

            var attackableSquaresIncludingSelfCaptures = rayType == PieceType.Rook
                ? AttackBitmaps.RookAttacks[fromSquare.Index][magicIndex]
                : AttackBitmaps.BishopAttacks[fromSquare.Index][magicIndex];

            return attackableSquaresIncludingSelfCaptures;
        }

        private void ToOrdinaryMoves(MoveGenerationWorkspace workspace, PieceType pieceType, Square fromSquare, SquareFlag attackableSquares)
        {
            var attackableSquaresAsList = attackableSquares.ToList();

            foreach (var toSquare in attackableSquaresAsList)
            {
                if (!workspace.RelativeBitBoard.OpponentSquares.HasFlag(toSquare))
                {
                    workspace.NonCaptureMoveBuffer.Add(MoveBuilder.Create(workspace.Colour, pieceType, fromSquare, toSquare.ToSquare(), PieceType.None, MoveType.Ordinary, workspace.NumCheckers));

                    continue;
                }

                var capturePieceType = workspace.RelativeBitBoard.GetPieceType(toSquare);

                workspace.CaptureMoveBuffer.Add(MoveBuilder.Create(workspace.Colour, pieceType, fromSquare, toSquare.ToSquare(), capturePieceType, MoveType.Ordinary, workspace.NumCheckers));
            }
        }
    }
}
