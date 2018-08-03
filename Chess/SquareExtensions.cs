namespace Chess
{
    public static class SquareExtensions
    {
        public static SquareFlag ToSquareFlag(this Square square)
        {
            if (square.Rank == 1 && square.File == File.a) return SquareFlag.A1;
            if (square.Rank == 1 && square.File == File.b) return SquareFlag.B1;
            if (square.Rank == 1 && square.File == File.c) return SquareFlag.C1;
            if (square.Rank == 1 && square.File == File.d) return SquareFlag.D1;
            if (square.Rank == 1 && square.File == File.e) return SquareFlag.E1;
            if (square.Rank == 1 && square.File == File.f) return SquareFlag.F1;
            if (square.Rank == 1 && square.File == File.g) return SquareFlag.G1;
            if (square.Rank == 1 && square.File == File.h) return SquareFlag.H1;
            if (square.Rank == 2 && square.File == File.a) return SquareFlag.A2;
            if (square.Rank == 2 && square.File == File.b) return SquareFlag.B2;
            if (square.Rank == 2 && square.File == File.c) return SquareFlag.C2;
            if (square.Rank == 2 && square.File == File.d) return SquareFlag.D2;
            if (square.Rank == 2 && square.File == File.e) return SquareFlag.E2;
            if (square.Rank == 2 && square.File == File.f) return SquareFlag.F2;
            if (square.Rank == 2 && square.File == File.g) return SquareFlag.G2;
            if (square.Rank == 2 && square.File == File.h) return SquareFlag.H2;
            if (square.Rank == 3 && square.File == File.a) return SquareFlag.A3;
            if (square.Rank == 3 && square.File == File.b) return SquareFlag.B3;
            if (square.Rank == 3 && square.File == File.c) return SquareFlag.C3;
            if (square.Rank == 3 && square.File == File.d) return SquareFlag.D3;
            if (square.Rank == 3 && square.File == File.e) return SquareFlag.E3;
            if (square.Rank == 3 && square.File == File.f) return SquareFlag.F3;
            if (square.Rank == 3 && square.File == File.g) return SquareFlag.G3;
            if (square.Rank == 3 && square.File == File.h) return SquareFlag.H3;
            if (square.Rank == 4 && square.File == File.a) return SquareFlag.A4;
            if (square.Rank == 4 && square.File == File.b) return SquareFlag.B4;
            if (square.Rank == 4 && square.File == File.c) return SquareFlag.C4;
            if (square.Rank == 4 && square.File == File.d) return SquareFlag.D4;
            if (square.Rank == 4 && square.File == File.e) return SquareFlag.E4;
            if (square.Rank == 4 && square.File == File.f) return SquareFlag.F4;
            if (square.Rank == 4 && square.File == File.g) return SquareFlag.G4;
            if (square.Rank == 4 && square.File == File.h) return SquareFlag.H4;
            if (square.Rank == 5 && square.File == File.a) return SquareFlag.A5;
            if (square.Rank == 5 && square.File == File.b) return SquareFlag.B5;
            if (square.Rank == 5 && square.File == File.c) return SquareFlag.C5;
            if (square.Rank == 5 && square.File == File.d) return SquareFlag.D5;
            if (square.Rank == 5 && square.File == File.e) return SquareFlag.E5;
            if (square.Rank == 5 && square.File == File.f) return SquareFlag.F5;
            if (square.Rank == 5 && square.File == File.g) return SquareFlag.G5;
            if (square.Rank == 5 && square.File == File.h) return SquareFlag.H5;
            if (square.Rank == 6 && square.File == File.a) return SquareFlag.A6;
            if (square.Rank == 6 && square.File == File.b) return SquareFlag.B6;
            if (square.Rank == 6 && square.File == File.c) return SquareFlag.C6;
            if (square.Rank == 6 && square.File == File.d) return SquareFlag.D6;
            if (square.Rank == 6 && square.File == File.e) return SquareFlag.E6;
            if (square.Rank == 6 && square.File == File.f) return SquareFlag.F6;
            if (square.Rank == 6 && square.File == File.g) return SquareFlag.G6;
            if (square.Rank == 6 && square.File == File.h) return SquareFlag.H6;
            if (square.Rank == 7 && square.File == File.a) return SquareFlag.A7;
            if (square.Rank == 7 && square.File == File.b) return SquareFlag.B7;
            if (square.Rank == 7 && square.File == File.c) return SquareFlag.C7;
            if (square.Rank == 7 && square.File == File.d) return SquareFlag.D7;
            if (square.Rank == 7 && square.File == File.e) return SquareFlag.E7;
            if (square.Rank == 7 && square.File == File.f) return SquareFlag.F7;
            if (square.Rank == 7 && square.File == File.g) return SquareFlag.G7;
            if (square.Rank == 7 && square.File == File.h) return SquareFlag.H7;
            if (square.Rank == 8 && square.File == File.a) return SquareFlag.A8;
            if (square.Rank == 8 && square.File == File.b) return SquareFlag.B8;
            if (square.Rank == 8 && square.File == File.c) return SquareFlag.C8;
            if (square.Rank == 8 && square.File == File.d) return SquareFlag.D8;
            if (square.Rank == 8 && square.File == File.e) return SquareFlag.E8;
            if (square.Rank == 8 && square.File == File.f) return SquareFlag.F8;
            if (square.Rank == 8 && square.File == File.g) return SquareFlag.G8;
            if (square.Rank == 8 && square.File == File.h) return SquareFlag.H8;
            return 0;
        }
    }
}
