# ChessSharp

ChessSharp is a basic C# based Chess game (note *game* not *engine* as there is not yet a [UCI](https://en.wikipedia.org/wiki/Universal_Chess_Interface) compatible component). It started as a little experiment when my son took an interest in Chess and then grew rapidly as I started to research how 'real' Chess programs worked.

As I experimented it became clear that Chess programming is an art in its own right and touches on many different aspects of programming but also that many examples I encountered required high levels of understanding due to their highly optimised implementations. In my day job I take an interest in implementing clean programming techniques so the goal for this project became:
* Identifying areas for use of programming techniques e.g. trying to keep classes small, variable names that convey purpose, using the latest C# syntax.
* Trying to keep a *reasonable* level of performance (turns out searching to ply 7 is harder than I initially thought).

On my development machine the code searches about 500k nodes per second (and depending on you're background that performance may seem extremely fast or extremely slow :-)). 


## C# and programming techniques summary
* [.NET Standard 2.0](https://docs.microsoft.com/en-us/dotnet/standard/net-standard) based projects.
* [Expression-bodied memebers](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/expression-bodied-members)
* [StyleCop](https://github.com/StyleCop/StyleCop) used to enforce code consistency.
* [System.Numerics.Vector](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector-1?view=netcore-2.2) to allow a small amount of parallel computation.


## Chess programming specific feature summary
* [Principal variation search](https://www.chessprogramming.org/Principal_Variation)
* [Transposition table](https://www.chessprogramming.org/Transposition_Table)
* [Zobrist hash keys](https://www.chessprogramming.org/Zobrist_Hashing)
* [Legal Move generation tool](https://www.chessprogramming.org/Move_Generation). Generates roughly 3.5 million moves per second on an AMD Ryzen 2600 (it's faster than 'in-game' because the nodes are not evaluated).
* That uses [a form of magic numbers based move generation](https://www.chessprogramming.org/Magic_Bitboards).


## What's missing
* [UCI](https://en.wikipedia.org/wiki/Universal_Chess_Interface) compatible component
* [Quiescence search] (https://www.chessprogramming.org/Quiescence_Search)
* Draw detection within search. The game will detect a draw one a move has been played, it does not however detect draws during the search i.e. it just burns time investigating positions that are already drawn.


## Getting Started

Clone the project to your local machine and launch in your IDE.


## Running the tests

These are xUnit based tests so you may need to install xUnit to run them.


## Authors

* **Greg Stanley** - *Initial work* - [Greg Stanley](https://github.com/gregstanley)


## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details


## Acknowledgments

* [CPW](https://www.chessprogramming.org/Main_Page)
* README.md template from [PurpleBooth](https://gist.githubusercontent.com/PurpleBooth/109311bb0361f32d87a2/raw/8254b53ab8dcb18afc64287aaddd9e5b6059f880/README-Template.md)
* Peter Ellis Jones for [this Generating Legal Chess Moves article](https://peterellisjones.com/posts/generating-legal-chess-moves-efficiently/) that I must have read ten times or more.
* [JetChess](https://zipproth.de/jetchess/) notes as inspiration and reference.
* [Thomas Petzke](https://macechess.blogspot.com/?m=1) and his mACE/iCE chess programming journey blog.
* [Jaco van Neikerk's magic numbers article](http://vicki-chess.blogspot.com/2013/04/magics.html) which really helped me get my head round it.