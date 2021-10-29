# ChessSharp

ChessSharp is a basic C# based Chess game (note *game* not *engine* as the [UCI](https://en.wikipedia.org/wiki/Universal_Chess_Interface) compatible component is just bare bones at the moment).
It started as a little experiment when my son took an interest in Chess and then grew rapidly as I started to research how 'real' Chess programs worked.
While experimenting it became clear that programming Chess is an art in it's own right and uses skills from all aspects of the discipline.
The many years of academic research in the area has led to many advanced solutions to problems you don't even realise you have when you start but fortunately there is a lot of guidance out there to help.
That said, I found many of the examples I encountered hard to get to grips with due to their highly optimised implementations.
I also have a general interest in making sure any code I produce not only does the job but is also as simple and easy to access as possible and techniques so I set myself the goal of trying to produce a project to demonstrate: 
* Appropriate use of the latest C# syntax and general best practise thinking.
* Simple implementations of core Chess programming concepts.
While trying to keep a *reasonable* level of performance (this code is not intended to beat [Stockfish](https://stockfishchess.org/) but it turns out searching to ply 7 is harder than I initially thought).
On my development machine the code searches about 500k nodes per second (and depending on your background that performance may seem extremely fast or extremely slow :-)). 


## C# and programming techniques summary
* [.NET 5.0](https://docs.microsoft.com/en-us/dotnet/standard/net-standard#net-5-and-net-standard) based projects.
* [Expression-bodied members](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/expression-bodied-members)
* [System.Numerics.Vector](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector-1?view=netcore-2.2) to allow a small amount of parallel computation.


## Chess programming specific feature summary
* [Principal variation search](https://www.chessprogramming.org/Principal_Variation)
* [Transposition table](https://www.chessprogramming.org/Transposition_Table)
* [Zobrist hash keys](https://www.chessprogramming.org/Zobrist_Hashing)
* [Legal Move generation tool](https://www.chessprogramming.org/Move_Generation). Generates roughly 3.5 million moves per second on an AMD Ryzen 2600 (it's faster than 'in-game' because the nodes are not evaluated).
* That uses [a form of magic numbers based move generation](https://www.chessprogramming.org/Magic_Bitboards).


## In progress
* [UCI](https://en.wikipedia.org/wiki/Universal_Chess_Interface) compatible component.


## What's missing
* [Quiescence search](https://www.chessprogramming.org/Quiescence_Search)
* 'Match drawn' detection within search. The game will detect a draw one a move has been played, it does not however detect draws during the search i.e. it just burns time investigating positions that are already drawn.


## Getting Started

Clone the project to your local machine and launch in your IDE.

Source (src) projects:

* ChessSharp Perft - Executable to test the perfromance of the move generation logic.
* ChessSharp UCI Engine - Executable [UCI](https://en.wikipedia.org/wiki/Universal_Chess_Interface) engine.
* ChessSharp UI - [WPF](https://docs.microsoft.com/en-us/dotnet/framework/wpf/getting-started/introduction-to-wpf-in-vs) based user interface for playing games of Chess (I'm not that familiar with [WPF](https://docs.microsoft.com/en-us/dotnet/framework/wpf/getting-started/introduction-to-wpf-in-vs) so I'm sure there's plenty of scope for improvement here).
* ChessSharp.Common - Core types and data structures.
* ChessSharp.Engine - All logic that is not directly move generation related.
* ChessSharp.MoveGeneration - Legal move generation.
* UCI Explorer - Console application to load [UCI](https://en.wikipedia.org/wiki/Universal_Chess_Interface) engines.

Test (tests) projects:
* ChessSharp Tests - [xUnit](https://xunit.github.io/docs/getting-started/netfx/visual-studio)


## Running the tests

These are [xUnit](https://xunit.net/) based tests so you may need to install [xUnit](https://xunit.net/) to run them.

## Test cases

Mate in two:
Fen: 8/6K1/1p1B1RB1/8/2Q5/2n1kP1N/3b4/4n3 w - - 0 1

* Bd6a3
* e4#

More: https://sites.google.com/site/darktemplarchess/mate-in-2-puzzles

## Authors

* **Greg Stanley** - *Initial work* - [Greg Stanley](https://github.com/gregstanley)


## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.


## Acknowledgments

* [CPW](https://www.chessprogramming.org/Main_Page) for the hundreds of in-depth definitions and examples.
* Peter Ellis Jones for [this Generating Legal Chess Moves article](https://peterellisjones.com/posts/generating-legal-chess-moves-efficiently/) that I must have read ten times or more.
* [JetChess](https://zipproth.de/jetchess/) notes as inspiration and reference.
* [Thomas Petzke](https://macechess.blogspot.com/?m=1) and his mACE/iCE chess programming journey blog.
* [Jaco van Neikerk's magic numbers article](http://vicki-chess.blogspot.com/2013/04/magics.html) which really helped me get my head round a rather unusual topic.
* README.md template from [PurpleBooth](https://gist.githubusercontent.com/PurpleBooth/109311bb0361f32d87a2/raw/8254b53ab8dcb18afc64287aaddd9e5b6059f880/README-Template.md)


## Known issues

* Doesn't show final move in on Checkmate