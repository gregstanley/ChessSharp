namespace Chess.Engine.Models
{
    public class RankFile
    {
        public static readonly RankFile A1 = new RankFile(1, File.a);
        public static readonly RankFile A2 = new RankFile(2, File.a);
        public static readonly RankFile A3 = new RankFile(3, File.a);
        public static readonly RankFile A4 = new RankFile(4, File.a);
        public static readonly RankFile A5 = new RankFile(5, File.a);
        public static readonly RankFile A6 = new RankFile(6, File.a);
        public static readonly RankFile A7 = new RankFile(7, File.a);
        public static readonly RankFile A8 = new RankFile(8, File.a);
        public static readonly RankFile B1 = new RankFile(1, File.b);
        public static readonly RankFile B2 = new RankFile(2, File.b);
        public static readonly RankFile B3 = new RankFile(3, File.b);
        public static readonly RankFile B4 = new RankFile(4, File.b);
        public static readonly RankFile B5 = new RankFile(5, File.b);
        public static readonly RankFile B6 = new RankFile(6, File.b);
        public static readonly RankFile B7 = new RankFile(7, File.b);
        public static readonly RankFile B8 = new RankFile(8, File.b);
        public static readonly RankFile C1 = new RankFile(1, File.c);
        public static readonly RankFile C2 = new RankFile(2, File.c);
        public static readonly RankFile C3 = new RankFile(3, File.c);
        public static readonly RankFile C4 = new RankFile(4, File.c);
        public static readonly RankFile C5 = new RankFile(5, File.c);
        public static readonly RankFile C6 = new RankFile(6, File.c);
        public static readonly RankFile C7 = new RankFile(7, File.c);
        public static readonly RankFile C8 = new RankFile(8, File.c);
        public static readonly RankFile D1 = new RankFile(1, File.d);
        public static readonly RankFile D2 = new RankFile(2, File.d);
        public static readonly RankFile D3 = new RankFile(3, File.d);
        public static readonly RankFile D4 = new RankFile(4, File.d);
        public static readonly RankFile D5 = new RankFile(5, File.d);
        public static readonly RankFile D6 = new RankFile(6, File.d);
        public static readonly RankFile D7 = new RankFile(7, File.d);
        public static readonly RankFile D8 = new RankFile(8, File.d);
        public static readonly RankFile E1 = new RankFile(1, File.e);
        public static readonly RankFile E2 = new RankFile(2, File.e);
        public static readonly RankFile E3 = new RankFile(3, File.e);
        public static readonly RankFile E4 = new RankFile(4, File.e);
        public static readonly RankFile E5 = new RankFile(5, File.e);
        public static readonly RankFile E6 = new RankFile(6, File.e);
        public static readonly RankFile E7 = new RankFile(7, File.e);
        public static readonly RankFile E8 = new RankFile(8, File.e);
        public static readonly RankFile F1 = new RankFile(1, File.f);
        public static readonly RankFile F2 = new RankFile(2, File.f);
        public static readonly RankFile F3 = new RankFile(3, File.f);
        public static readonly RankFile F4 = new RankFile(4, File.f);
        public static readonly RankFile F5 = new RankFile(5, File.f);
        public static readonly RankFile F6 = new RankFile(6, File.f);
        public static readonly RankFile F7 = new RankFile(7, File.f);
        public static readonly RankFile F8 = new RankFile(8, File.f);
        public static readonly RankFile G1 = new RankFile(1, File.g);
        public static readonly RankFile G2 = new RankFile(2, File.g);
        public static readonly RankFile G3 = new RankFile(3, File.g);
        public static readonly RankFile G4 = new RankFile(4, File.g);
        public static readonly RankFile G5 = new RankFile(5, File.g);
        public static readonly RankFile G6 = new RankFile(6, File.g);
        public static readonly RankFile G7 = new RankFile(7, File.g);
        public static readonly RankFile G8 = new RankFile(8, File.g);
        public static readonly RankFile H1 = new RankFile(1, File.h);
        public static readonly RankFile H2 = new RankFile(2, File.h);
        public static readonly RankFile H3 = new RankFile(3, File.h);
        public static readonly RankFile H4 = new RankFile(4, File.h);
        public static readonly RankFile H5 = new RankFile(5, File.h);
        public static readonly RankFile H6 = new RankFile(6, File.h);
        public static readonly RankFile H7 = new RankFile(7, File.h);
        public static readonly RankFile H8 = new RankFile(8, File.h);

        public int Rank { get; }

        public File File { get; }

        public SquareFlag ToSquareFlag()
        {
            switch (File)
            {
                case File.a when Rank == 1:
                    return SquareFlag.A1;
                case File.b when Rank == 1:
                    return SquareFlag.B1;
                case File.c when Rank == 1:
                    return SquareFlag.C1;
                case File.d when Rank == 1:
                    return SquareFlag.D1;
                case File.e when Rank == 1:
                    return SquareFlag.E1;
                case File.f when Rank == 1:
                    return SquareFlag.F1;
                case File.g when Rank == 1:
                    return SquareFlag.G1;
                case File.h when Rank == 1:
                    return SquareFlag.H1;
                case File.a when Rank == 2:
                    return SquareFlag.A2;
                case File.b when Rank == 2:
                    return SquareFlag.B2;
                case File.c when Rank == 2:
                    return SquareFlag.C2;
                case File.d when Rank == 2:
                    return SquareFlag.D2;
                case File.e when Rank == 2:
                    return SquareFlag.E2;
                case File.f when Rank == 2:
                    return SquareFlag.F2;
                case File.g when Rank == 2:
                    return SquareFlag.G2;
                case File.h when Rank == 2:
                    return SquareFlag.H2;
                case File.a when Rank == 3:
                    return SquareFlag.A3;
                case File.b when Rank == 3:
                    return SquareFlag.B3;
                case File.c when Rank == 3:
                    return SquareFlag.C3;
                case File.d when Rank == 3:
                    return SquareFlag.D3;
                case File.e when Rank == 3:
                    return SquareFlag.E3;
                case File.f when Rank == 3:
                    return SquareFlag.F3;
                case File.g when Rank == 3:
                    return SquareFlag.G3;
                case File.h when Rank == 3:
                    return SquareFlag.H3;
                case File.a when Rank == 4:
                    return SquareFlag.A4;
                case File.b when Rank == 4:
                    return SquareFlag.B4;
                case File.c when Rank == 4:
                    return SquareFlag.C4;
                case File.d when Rank == 4:
                    return SquareFlag.D4;
                case File.e when Rank == 4:
                    return SquareFlag.E4;
                case File.f when Rank == 4:
                    return SquareFlag.F4;
                case File.g when Rank == 4:
                    return SquareFlag.G4;
                case File.h when Rank == 4:
                    return SquareFlag.H4;
                case File.a when Rank == 5:
                    return SquareFlag.A5;
                case File.b when Rank == 5:
                    return SquareFlag.B5;
                case File.c when Rank == 5:
                    return SquareFlag.C5;
                case File.d when Rank == 5:
                    return SquareFlag.D5;
                case File.e when Rank == 5:
                    return SquareFlag.E5;
                case File.f when Rank == 5:
                    return SquareFlag.F5;
                case File.g when Rank == 5:
                    return SquareFlag.G5;
                case File.h when Rank == 5:
                    return SquareFlag.H5;
                case File.a when Rank == 6:
                    return SquareFlag.A6;
                case File.b when Rank == 6:
                    return SquareFlag.B6;
                case File.c when Rank == 6:
                    return SquareFlag.C6;
                case File.d when Rank == 6:
                    return SquareFlag.D6;
                case File.e when Rank == 6:
                    return SquareFlag.E6;
                case File.f when Rank == 6:
                    return SquareFlag.F6;
                case File.g when Rank == 6:
                    return SquareFlag.G6;
                case File.h when Rank == 6:
                    return SquareFlag.H6;
                case File.a when Rank == 7:
                    return SquareFlag.A7;
                case File.b when Rank == 7:
                    return SquareFlag.B7;
                case File.c when Rank == 7:
                    return SquareFlag.C7;
                case File.d when Rank == 7:
                    return SquareFlag.D7;
                case File.e when Rank == 7:
                    return SquareFlag.E7;
                case File.f when Rank == 7:
                    return SquareFlag.F7;
                case File.g when Rank == 7:
                    return SquareFlag.G7;
                case File.h when Rank == 7:
                    return SquareFlag.H7;
                case File.a when Rank == 8:
                    return SquareFlag.A8;
                case File.b when Rank == 8:
                    return SquareFlag.B8;
                case File.c when Rank == 8:
                    return SquareFlag.C8;
                case File.d when Rank == 8:
                    return SquareFlag.D8;
                case File.e when Rank == 8:
                    return SquareFlag.E8;
                case File.f when Rank == 8:
                    return SquareFlag.F8;
                case File.g when Rank == 8:
                    return SquareFlag.G8;
                case File.h when Rank == 8:
                    return SquareFlag.H8;
            }
            return 0;
        }

        private RankFile(int rank, File file)
        {
            Rank = rank;
            File = file;
        }

        public static RankFile Get(int rank, File file)
        {
            switch (file)
            {
                case File.a when rank == 1:
                    return A1;
                case File.b when rank == 1:
                    return B1;
                case File.c when rank == 1:
                    return C1;
                case File.d when rank == 1:
                    return D1;
                case File.e when rank == 1:
                    return E1;
                case File.f when rank == 1:
                    return F1;
                case File.g when rank == 1:
                    return G1;
                case File.h when rank == 1:
                    return H1;
                case File.a when rank == 2:
                    return A2;
                case File.b when rank == 2:
                    return B2;
                case File.c when rank == 2:
                    return C2;
                case File.d when rank == 2:
                    return D2;
                case File.e when rank == 2:
                    return E2;
                case File.f when rank == 2:
                    return F2;
                case File.g when rank == 2:
                    return G2;
                case File.h when rank == 2:
                    return H2;
                case File.a when rank == 3:
                    return A3;
                case File.b when rank == 3:
                    return B3;
                case File.c when rank == 3:
                    return C3;
                case File.d when rank == 3:
                    return D3;
                case File.e when rank == 3:
                    return E3;
                case File.f when rank == 3:
                    return F3;
                case File.g when rank == 3:
                    return G3;
                case File.h when rank == 3:
                    return H3;
                case File.a when rank == 4:
                    return A4;
                case File.b when rank == 4:
                    return B4;
                case File.c when rank == 4:
                    return C4;
                case File.d when rank == 4:
                    return D4;
                case File.e when rank == 4:
                    return E4;
                case File.f when rank == 4:
                    return F4;
                case File.g when rank == 4:
                    return G4;
                case File.h when rank == 4:
                    return H4;
                case File.a when rank == 5:
                    return A5;
                case File.b when rank == 5:
                    return B5;
                case File.c when rank == 5:
                    return C5;
                case File.d when rank == 5:
                    return D5;
                case File.e when rank == 5:
                    return E5;
                case File.f when rank == 5:
                    return F5;
                case File.g when rank == 5:
                    return G5;
                case File.h when rank == 5:
                    return H5;
                case File.a when rank == 6:
                    return A6;
                case File.b when rank == 6:
                    return B6;
                case File.c when rank == 6:
                    return C6;
                case File.d when rank == 6:
                    return D6;
                case File.e when rank == 6:
                    return E6;
                case File.f when rank == 6:
                    return F6;
                case File.g when rank == 6:
                    return G6;
                case File.h when rank == 6:
                    return H6;
                case File.a when rank == 7:
                    return A7;
                case File.b when rank == 7:
                    return B7;
                case File.c when rank == 7:
                    return C7;
                case File.d when rank == 7:
                    return D7;
                case File.e when rank == 7:
                    return E7;
                case File.f when rank == 7:
                    return F7;
                case File.g when rank == 7:
                    return G7;
                case File.h when rank == 7:
                    return H7;
                case File.a when rank == 8:
                    return A8;
                case File.b when rank == 8:
                    return B8;
                case File.c when rank == 8:
                    return C8;
                case File.d when rank == 8:
                    return D8;
                case File.e when rank == 8:
                    return E8;
                case File.f when rank == 8:
                    return F8;
                case File.g when rank == 8:
                    return G8;
                case File.h when rank == 8:
                    return H8;
            }
            return null;
        }

        public RelativePosition To(RankFile rankFile)
        {
            var rank = rankFile.Rank - Rank;
            var file = rankFile.File - File;
                
            return new RelativePosition(rank, file);
        }

        public override string ToString() =>
            $"{File}{Rank}";
    }
}
