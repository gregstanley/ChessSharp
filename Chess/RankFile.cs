namespace Chess
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
            if (File == File.a && Rank == 1) return SquareFlag.A1;
            if (File == File.b && Rank == 1) return SquareFlag.B1;
            if (File == File.c && Rank == 1) return SquareFlag.C1;
            if (File == File.d && Rank == 1) return SquareFlag.D1;
            if (File == File.e && Rank == 1) return SquareFlag.E1;
            if (File == File.f && Rank == 1) return SquareFlag.F1;
            if (File == File.g && Rank == 1) return SquareFlag.G1;
            if (File == File.h && Rank == 1) return SquareFlag.H1;
            if (File == File.a && Rank == 2) return SquareFlag.A2;
            if (File == File.b && Rank == 2) return SquareFlag.B2;
            if (File == File.c && Rank == 2) return SquareFlag.C2;
            if (File == File.d && Rank == 2) return SquareFlag.D2;
            if (File == File.e && Rank == 2) return SquareFlag.E2;
            if (File == File.f && Rank == 2) return SquareFlag.F2;
            if (File == File.g && Rank == 2) return SquareFlag.G2;
            if (File == File.h && Rank == 2) return SquareFlag.H2;
            if (File == File.a && Rank == 3) return SquareFlag.A3;
            if (File == File.b && Rank == 3) return SquareFlag.B3;
            if (File == File.c && Rank == 3) return SquareFlag.C3;
            if (File == File.d && Rank == 3) return SquareFlag.D3;
            if (File == File.e && Rank == 3) return SquareFlag.E3;
            if (File == File.f && Rank == 3) return SquareFlag.F3;
            if (File == File.g && Rank == 3) return SquareFlag.G3;
            if (File == File.h && Rank == 3) return SquareFlag.H3;
            if (File == File.a && Rank == 4) return SquareFlag.A4;
            if (File == File.b && Rank == 4) return SquareFlag.B4;
            if (File == File.c && Rank == 4) return SquareFlag.C4;
            if (File == File.d && Rank == 4) return SquareFlag.D4;
            if (File == File.e && Rank == 4) return SquareFlag.E4;
            if (File == File.f && Rank == 4) return SquareFlag.F4;
            if (File == File.g && Rank == 4) return SquareFlag.G4;
            if (File == File.h && Rank == 4) return SquareFlag.H4;
            if (File == File.a && Rank == 5) return SquareFlag.A5;
            if (File == File.b && Rank == 5) return SquareFlag.B5;
            if (File == File.c && Rank == 5) return SquareFlag.C5;
            if (File == File.d && Rank == 5) return SquareFlag.D5;
            if (File == File.e && Rank == 5) return SquareFlag.E5;
            if (File == File.f && Rank == 5) return SquareFlag.F5;
            if (File == File.g && Rank == 5) return SquareFlag.G5;
            if (File == File.h && Rank == 5) return SquareFlag.H5;
            if (File == File.a && Rank == 6) return SquareFlag.A6;
            if (File == File.b && Rank == 6) return SquareFlag.B6;
            if (File == File.c && Rank == 6) return SquareFlag.C6;
            if (File == File.d && Rank == 6) return SquareFlag.D6;
            if (File == File.e && Rank == 6) return SquareFlag.E6;
            if (File == File.f && Rank == 6) return SquareFlag.F6;
            if (File == File.g && Rank == 6) return SquareFlag.G6;
            if (File == File.h && Rank == 6) return SquareFlag.H6;
            if (File == File.a && Rank == 7) return SquareFlag.A7;
            if (File == File.b && Rank == 7) return SquareFlag.B7;
            if (File == File.c && Rank == 7) return SquareFlag.C7;
            if (File == File.d && Rank == 7) return SquareFlag.D7;
            if (File == File.e && Rank == 7) return SquareFlag.E7;
            if (File == File.f && Rank == 7) return SquareFlag.F7;
            if (File == File.g && Rank == 7) return SquareFlag.G7;
            if (File == File.h && Rank == 7) return SquareFlag.H7;
            if (File == File.a && Rank == 8) return SquareFlag.A8;
            if (File == File.b && Rank == 8) return SquareFlag.B8;
            if (File == File.c && Rank == 8) return SquareFlag.C8;
            if (File == File.d && Rank == 8) return SquareFlag.D8;
            if (File == File.e && Rank == 8) return SquareFlag.E8;
            if (File == File.f && Rank == 8) return SquareFlag.F8;
            if (File == File.g && Rank == 8) return SquareFlag.G8;
            if (File == File.h && Rank == 8) return SquareFlag.H8;
            return 0;
        }

        private RankFile(int rank, File file)
        {
            Rank = rank;
            File = file;
        }

        public static RankFile Get(int rank, File file)
        {
            if (file == File.a && rank == 1) return A1;
            if (file == File.b && rank == 1) return B1;
            if (file == File.c && rank == 1) return C1;
            if (file == File.d && rank == 1) return D1;
            if (file == File.e && rank == 1) return E1;
            if (file == File.f && rank == 1) return F1;
            if (file == File.g && rank == 1) return G1;
            if (file == File.h && rank == 1) return H1;
            if (file == File.a && rank == 2) return A2;
            if (file == File.b && rank == 2) return B2;
            if (file == File.c && rank == 2) return C2;
            if (file == File.d && rank == 2) return D2;
            if (file == File.e && rank == 2) return E2;
            if (file == File.f && rank == 2) return F2;
            if (file == File.g && rank == 2) return G2;
            if (file == File.h && rank == 2) return H2;
            if (file == File.a && rank == 3) return A3;
            if (file == File.b && rank == 3) return B3;
            if (file == File.c && rank == 3) return C3;
            if (file == File.d && rank == 3) return D3;
            if (file == File.e && rank == 3) return E3;
            if (file == File.f && rank == 3) return F3;
            if (file == File.g && rank == 3) return G3;
            if (file == File.h && rank == 3) return H3;
            if (file == File.a && rank == 4) return A4;
            if (file == File.b && rank == 4) return B4;
            if (file == File.c && rank == 4) return C4;
            if (file == File.d && rank == 4) return D4;
            if (file == File.e && rank == 4) return E4;
            if (file == File.f && rank == 4) return F4;
            if (file == File.g && rank == 4) return G4;
            if (file == File.h && rank == 4) return H4;
            if (file == File.a && rank == 5) return A5;
            if (file == File.b && rank == 5) return B5;
            if (file == File.c && rank == 5) return C5;
            if (file == File.d && rank == 5) return D5;
            if (file == File.e && rank == 5) return E5;
            if (file == File.f && rank == 5) return F5;
            if (file == File.g && rank == 5) return G5;
            if (file == File.h && rank == 5) return H5;
            if (file == File.a && rank == 6) return A6;
            if (file == File.b && rank == 6) return B6;
            if (file == File.c && rank == 6) return C6;
            if (file == File.d && rank == 6) return D6;
            if (file == File.e && rank == 6) return E6;
            if (file == File.f && rank == 6) return F6;
            if (file == File.g && rank == 6) return G6;
            if (file == File.h && rank == 6) return H6;
            if (file == File.a && rank == 7) return A7;
            if (file == File.b && rank == 7) return B7;
            if (file == File.c && rank == 7) return C7;
            if (file == File.d && rank == 7) return D7;
            if (file == File.e && rank == 7) return E7;
            if (file == File.f && rank == 7) return F7;
            if (file == File.g && rank == 7) return G7;
            if (file == File.h && rank == 7) return H7;
            if (file == File.a && rank == 8) return A8;
            if (file == File.b && rank == 8) return B8;
            if (file == File.c && rank == 8) return C8;
            if (file == File.d && rank == 8) return D8;
            if (file == File.e && rank == 8) return E8;
            if (file == File.f && rank == 8) return F8;
            if (file == File.g && rank == 8) return G8;
            if (file == File.h && rank == 8) return H8;
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
