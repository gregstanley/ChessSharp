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
            workspaces = new MoveGenerationWorkspace[1];

            workspaces[0] = new MoveGenerationWorkspace(0);
        }

        public MoveGenerator(int workspaceCount)
        {
            workspaces = new MoveGenerationWorkspace[workspaceCount];

            for (var i = 0; i < workspaceCount; ++i)
                workspaces[i] = new MoveGenerationWorkspace(i);
        }

        public void Generate(BitBoard bitBoard, Colour colour, IList<uint> moves)
        {
            foreach (var move in GenerateChunk(0, bitBoard, colour))
                moves.Add(move);
        }

        public void Generate(ushort depth, BitBoard bitBoard, Colour colour, IList<uint> moves)
        {
            if (depth >= workspaces.Length)
                throw new ArgumentException($"No buffers available for depth {depth}", nameof(depth));

            foreach (var move in GenerateChunk(depth, bitBoard, colour))
                moves.Add(move);
        }

        public IEnumerable<uint> GenerateChunk(ushort depth, BitBoard bitBoard, Colour colour)
        {
            if (depth >= workspaces.Length)
                throw new ArgumentException($"No buffers available for depth {depth}", nameof(depth));

            var workspace = workspaces[depth];

            var relativeBitBoard = workspace.Reset(bitBoard, colour);

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

            var safeSquaresAsList = FindSafeSquares(relativeBitBoard, workspace, semiSafeAttackableSquaresAsList);

            var numCheckers = checkers.Count();

            foreach (var toSquare in safeSquaresAsList)
            {
                if (relativeBitBoard.OpponentSquares.HasFlag(toSquare))
                {
                    var capturePieceType = relativeBitBoard.GetPieceType(toSquare);

                    var kingMove = MoveBuilder.Create(relativeBitBoard.Colour, PieceType.King, kingSquare, toSquare.ToSquare(), capturePieceType, MoveType.Ordinary, 0); // numCheckers);

                    workspace.KingCaptureMoveBuffer.Add(kingMove);
                }
                else
                {
                    var kingMove = MoveBuilder.Create(relativeBitBoard.Colour, PieceType.King, kingSquare, toSquare.ToSquare(), PieceType.None, MoveType.Ordinary, 0); // numCheckers);

                    workspace.KingNonCaptureMoveBuffer.Add(kingMove);
                }
            }

            // If in Check by more than one piece then only options are King moves - which we already have
            if (numCheckers > 1)
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

            if (numCheckers == 1)
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
                AddCastles(relativeBitBoard, workspace, kingSquare, workspace.NonCaptureMoveBuffer);
            }

            var kingRayAttackSquaresWithoutKing = kingRayAttackSquares & ~kingSquare.Flag;

            // Pass the relative bitboard in separately to avoid it being re-generated
            var pinnedSquares = AddPinnedMoves(workspace, relativeBitBoard, kingSquare, kingRayAttackSquaresWithoutKing, pushMask, captureMask, workspace.CaptureMoveBuffer, workspace.NonCaptureMoveBuffer);

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
                        AddIndividualPawnCaptures(relativeBitBoard, pieceSquare.ToSquare(), pushMask, captureMask, workspace.CaptureMoveBuffer, workspace.NonCaptureMoveBuffer);
                }

                foreach (var move in workspace.CaptureMoveBuffer)
                    yield return move;

                workspace.CaptureMoveBuffer.Clear();

                if (promotionPushes > 0)
                {
                    foreach (var pieceSquare in promotionPushes.ToList())
                    {
                        var fromSquare = pieceSquare.PawnBackward(relativeBitBoard.Colour, 1);

                        AddPromotions(relativeBitBoard, fromSquare.ToSquare(), pieceSquare.ToSquare(), PieceType.None, workspace.CaptureMoveBuffer, workspace.NonCaptureMoveBuffer);
                    }
                }

                if (nonPromotionPushes > 0)
                {
                    foreach (var pieceSquare in nonPromotionPushes.ToList())
                    {
                        var fromSquare = pieceSquare.PawnBackward(relativeBitBoard.Colour, 1);

                        workspace.NonCaptureMoveBuffer.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.Pawn, fromSquare.ToSquare(), pieceSquare.ToSquare(), PieceType.None, MoveType.Ordinary));
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

                            workspace.NonCaptureMoveBuffer.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.Pawn, fromSquare.ToSquare(), pieceSquare.ToSquare(), PieceType.None, MoveType.Ordinary));
                        }
                    }
                }
            }

            if (relativeBitBoard.MyKnights > 0)
            {
                foreach (var pieceSquareFlag in remainingKnights.ToList())
                    AddIndividualKnightMoves(relativeBitBoard, pieceSquareFlag.ToSquare(), legalMask, workspace.CaptureMoveBuffer, workspace.NonCaptureMoveBuffer);
            }

            foreach (var move in workspace.CaptureMoveBuffer)
                yield return move;

            workspace.CaptureMoveBuffer.Clear();

            if (relativeBitBoard.MyRooks > 0)
            {
                foreach (var pieceSquare in remainingRooks.ToList())
                    AddIndividualRayMoves(relativeBitBoard, pieceSquare.ToSquare(), PieceType.Rook, PieceType.Rook, legalMask, workspace.CaptureMoveBuffer, workspace.NonCaptureMoveBuffer);
            }

            foreach (var move in workspace.CaptureMoveBuffer)
                yield return move;

            workspace.CaptureMoveBuffer.Clear();

            if (relativeBitBoard.MyBishops > 0)
            {
                foreach (var pieceSquare in remainingBishops.ToList())
                    AddIndividualRayMoves(relativeBitBoard, pieceSquare.ToSquare(), PieceType.Bishop, PieceType.Bishop, legalMask, workspace.CaptureMoveBuffer, workspace.NonCaptureMoveBuffer);
            }

            foreach (var move in workspace.CaptureMoveBuffer)
                yield return move;

            workspace.CaptureMoveBuffer.Clear();

            if (relativeBitBoard.MyQueens > 0)
            {
                var remainingQueens = relativeBitBoard.MyQueens & remainingSquares;

                foreach (var pieceSquare in remainingQueens.ToList())
                    AddIndividualQueenMoves(relativeBitBoard, pieceSquare.ToSquare(), legalMask, workspace.CaptureMoveBuffer, workspace.NonCaptureMoveBuffer);
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

        private IList<SquareFlag> FindSafeSquares(RelativeBitBoard relativeBitBoard, MoveGenerationWorkspace workspace, IEnumerable<SquareFlag> attackableSquares)
        {
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
            RelativeBitBoard relativeBitBoard,
            Square kingSquare,
            SquareFlag kingRayAttackSquares,
            SquareFlag pushMask,
            SquareFlag captureMask,
            IList<uint> captureMoves,
            IList<uint> nonCaptureMoves)
        {
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
                pinnedPieces |= AddPinnedMovesInternal(relativeBitBoard, kingSquare, potentialPins, pinningRooks, false, pushMask, captureMask, captureMoves, nonCaptureMoves);

            if (pinningBishops > 0)
                pinnedPieces |= AddPinnedMovesInternal(relativeBitBoard, kingSquare, potentialPins, pinningBishops, true, pushMask, captureMask, captureMoves, nonCaptureMoves);

            if (pinningQueensNonDiagonal > 0)
                pinnedPieces |= AddPinnedMovesInternal(relativeBitBoard, kingSquare, potentialPins, pinningQueensNonDiagonal, false, pushMask, captureMask, captureMoves, nonCaptureMoves);

            if (pinningQueensDiagonal > 0)
                pinnedPieces |= AddPinnedMovesInternal(relativeBitBoard, kingSquare, potentialPins, pinningQueensDiagonal, true, pushMask, captureMask, captureMoves, nonCaptureMoves);

            return pinnedPieces;
        }

        private SquareFlag AddPinnedMovesInternal(
            RelativeBitBoard relativeBitBoard,
            Square kingSquare,
            SquareFlag potentialPins,
            SquareFlag pinners,
            bool diagonal,
            SquareFlag pushMask,
            SquareFlag captureMask,
            IList<uint> captureMoves,
            IList<uint> nonCaptureMoves)
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

                var pieceType = relativeBitBoard.GetPieceType(squarePinnedByThisPiece);

                switch (pieceType)
                {
                    case PieceType.Pawn when !diagonal:
                        AddIndividualPawnPushes(relativeBitBoard, squarePinnedByThisPiece.ToSquare(), pushPath, captureMoves, nonCaptureMoves);
                        break;
                    case PieceType.Pawn when diagonal:
                        AddIndividualPawnCaptures(relativeBitBoard, squarePinnedByThisPiece.ToSquare(), pushPath, capturePath, captureMoves, nonCaptureMoves);
                        break;
                    case PieceType.Rook:
                        AddIndividualRayMoves(relativeBitBoard, squarePinnedByThisPiece.ToSquare(), PieceType.Rook, PieceType.Rook, pushPath | capturePath, captureMoves, nonCaptureMoves);
                        break;
                    case PieceType.Bishop:
                        AddIndividualRayMoves(relativeBitBoard, squarePinnedByThisPiece.ToSquare(), PieceType.Bishop, PieceType.Bishop, pushPath | capturePath, captureMoves, nonCaptureMoves);
                        break;
                    case PieceType.Queen:
                        AddIndividualQueenMoves(relativeBitBoard, squarePinnedByThisPiece.ToSquare(), pushPath | capturePath, captureMoves, nonCaptureMoves);
                        break;
                }
            }

            return pinnedSquares;
        }
        
        private void AddIndividualPawnPushes(RelativeBitBoard relativeBitBoard, Square fromSquare, SquareFlag pushMask, IList<uint> captureMoves, IList<uint> nonCaptureMoves)
        {
            var toSquare = fromSquare.Flag
                .PawnForward(relativeBitBoard.Colour, 1)
                .ToSquare();

            if (relativeBitBoard.OccupiedSquares.HasFlag(toSquare.Flag))
                return;

            if (pushMask.HasFlag(toSquare.Flag))
            {
                if (relativeBitBoard.PromotionRank.HasFlag(toSquare.Flag))
                    AddPromotions(relativeBitBoard, fromSquare, toSquare, PieceType.None, captureMoves, nonCaptureMoves);
                else
                    nonCaptureMoves.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, PieceType.None, MoveType.Ordinary));
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
                    nonCaptureMoves.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, PieceType.None, MoveType.Ordinary));
            }
        }

        private void AddIndividualPawnCaptures(RelativeBitBoard relativeBitBoard, Square fromSquare, SquareFlag pushMask, SquareFlag captureMask, IList<uint> captureMoves, IList<uint> nonCaptureMoves)
        {
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
                        AddPromotions(relativeBitBoard, fromSquare, toSquare.ToSquare(), capturePieceType, captureMoves, nonCaptureMoves);
                    }
                    else if (!discoveredCheck)
                    {
                        if (capturePieceType != PieceType.None)
                            captureMoves.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare.ToSquare(), capturePieceType, moveType));
                        else
                            nonCaptureMoves.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare.ToSquare(), capturePieceType, moveType));
                    }
                }
            }
        }

        private void AddIndividualKnightMoves(RelativeBitBoard relativeBitBoard, Square fromSquare, SquareFlag legalMask, IList<uint> captureMoves, IList<uint> nonCaptureMoves)
        {
            var attackableSquaresIncludingSelfCaptures = AttackBitmaps.KnightAttacks[fromSquare.Index];
            var attackableSquares = attackableSquaresIncludingSelfCaptures & ~relativeBitBoard.MySquares;
            var legalAttackableSquares = attackableSquares & legalMask;

            ToOrdinaryMoves(relativeBitBoard, PieceType.Knight, fromSquare, legalAttackableSquares, captureMoves, nonCaptureMoves);
        }

        private void AddIndividualQueenMoves(RelativeBitBoard relativeBitBoard, Square fromSquare, SquareFlag legalMask, IList<uint> captureMoves, IList<uint> nonCaptureMoves)
        {
            AddIndividualRayMoves(relativeBitBoard, fromSquare, PieceType.Rook, PieceType.Queen, legalMask, captureMoves, nonCaptureMoves);
            AddIndividualRayMoves(relativeBitBoard, fromSquare, PieceType.Bishop, PieceType.Queen, legalMask, captureMoves, nonCaptureMoves);
        }

        private void AddIndividualRayMoves(RelativeBitBoard relativeBitBoard, Square fromSquare, PieceType rayType, PieceType pieceType, SquareFlag legalMask, IList<uint> captureMoves, IList<uint> nonCaptureMoves)
        {
            var attackableSquaresIncludingSelfCaptures = GetAttackableSquares(fromSquare, rayType, relativeBitBoard.OccupiedSquares);
            var attackableSquares = attackableSquaresIncludingSelfCaptures & ~relativeBitBoard.MySquares;
            var legalAttackableSquares = attackableSquares & legalMask;

            ToOrdinaryMoves(relativeBitBoard, pieceType, fromSquare, legalAttackableSquares, captureMoves, nonCaptureMoves);
        }

        private void AddCastles(RelativeBitBoard relativeBitBoard, MoveGenerationWorkspace workspace, Square kingSquare, IList<uint> moves)
        {
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
                    var safeSquaresAsList = FindSafeSquares(relativeBitBoard, workspace, stepSquares.ToList());

                    var safeSquares = (SquareFlag)0;

                    foreach (var safeSquare in safeSquaresAsList)
                        safeSquares |= safeSquare;

                    if (squaresBetween == safeSquares)
                    {
                        moves.Add(MoveBuilder.CreateCastle(relativeBitBoard.Colour, MoveType.CastleKing));
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
                    var safeSquaresAsList = FindSafeSquares(relativeBitBoard, workspace, stepSquares.ToList());

                    var safeSquares = (SquareFlag)0;

                    foreach (var safeSquare in safeSquaresAsList)
                        safeSquares |= safeSquare;

                    // On Queen side the King doesn't pass through B file so we don't look for Check there
                    var squaresBetweenMinusFirstRookStep = squaresBetween & ~relativeBitBoard.QueenSideRookStep1Square;

                    if (squaresBetweenMinusFirstRookStep == safeSquares)
                        moves.Add(MoveBuilder.CreateCastle(relativeBitBoard.Colour, MoveType.CastleQueen));
                }
            }
        }

        private void AddPromotions(RelativeBitBoard relativeBitBoard, Square fromSquare, Square toSquare, PieceType capturePieceType, IList<uint> captureMoves, IList<uint> nonCaptureMoves)
        {
            if (capturePieceType != PieceType.None)
            {
                captureMoves.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionQueen));
                captureMoves.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionRook));
                captureMoves.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionKnight));
                captureMoves.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionBishop));
            }
            else
            {
                nonCaptureMoves.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionQueen));
                nonCaptureMoves.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionRook));
                nonCaptureMoves.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionKnight));
                nonCaptureMoves.Add(MoveBuilder.Create(relativeBitBoard.Colour, PieceType.Pawn, fromSquare, toSquare, capturePieceType, MoveType.PromotionBishop));
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

        private void ToOrdinaryMoves(RelativeBitBoard relativeBitBoard, PieceType pieceType, Square fromSquare, SquareFlag attackableSquares, IList<uint> captureMoves, IList<uint> nonCaptureMoves)
        {
            var attackableSquaresAsList = attackableSquares.ToList();

            foreach (var toSquare in attackableSquaresAsList)
            {
                if (!relativeBitBoard.OpponentSquares.HasFlag(toSquare))
                {
                    nonCaptureMoves.Add(MoveBuilder.Create(relativeBitBoard.Colour, pieceType, fromSquare, toSquare.ToSquare(), PieceType.None, MoveType.Ordinary));

                    continue;
                }

                var capturePieceType = relativeBitBoard.GetPieceType(toSquare);

                captureMoves.Add(MoveBuilder.Create(relativeBitBoard.Colour, pieceType, fromSquare, toSquare.ToSquare(), capturePieceType, MoveType.Ordinary));
            }
        }
    }
}
