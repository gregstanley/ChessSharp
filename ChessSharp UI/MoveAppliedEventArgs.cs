using ChessSharp;
using System.Windows;

namespace ChessSharp_UI
{
    public class MoveAppliedEventArgs : RoutedEventArgs
    {
        public MoveAppliedEventArgs(MoveViewer move)
        {
            Move = move;
        }

        public MoveViewer Move { get; }
    }
}
