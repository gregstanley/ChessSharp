using System;

namespace ChessSharp.Engine.Events
{
    public class InfoEventArgs : EventArgs
    {
        public InfoEventArgs(Info info)
        {
            Info = info;
        }

        public Info Info { get; }
    }
}
