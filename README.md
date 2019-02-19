# ChessSharp

ChessSharp is a basic C# based Chess game (note *game* not *engine* as there is not yet a [UCI](https://en.wikipedia.org/wiki/Universal_Chess_Interface) compatible component). It started as a little experiment when my son took an interest in Chess and then grew rapidly as I started to research how 'real' Chess programs worked.

As I experimented it became clear that programming any Chess game touches on many different aspects of programming but also that many examples I encountered required high levels of understanding due to their highly optimised implementations. In my day job I take an interest in implementing clean programming techniques so the goal for this project became:
* Identifying areas for use of programming techniques i.e. keeping things in classes, clear variable names.
* Trying to keep a *reasonable* level of performance (turns out search to ply 7 is harder than I initially thought).

On my development machine the code searches about 500k nodes per second (and depending on you're background that performance may seem extremely fast or extremely slow :-)). 

## Feature summary
* AspNet Standard 2.0.
* [Principal variation search](https://www.chessprogramming.org/Principal_Variation)
* [Transposition table](https://www.chessprogramming.org/Transposition_Table)
* [Zobrist hash keys](https://www.chessprogramming.org/Zobrist_Hashing)

## What's missing
* [UCI](https://en.wikipedia.org/wiki/Universal_Chess_Interface) compatible component
* [Quiescence search] (https://www.chessprogramming.org/Quiescence_Search)

## Getting Started

Clone the project to your local machine and launch in Visual Studio (or your IDE of choice).


## Running the tests

These are xUnit based tests so you may need to install xUnit to run them.


## Contributing

TODO: Please read [CONTRIBUTING.md](https://gist.github.com/PurpleBooth/b24679402957c63ec426) for details on our code of conduct, and the process for submitting pull requests to us.

## Authors

* **Greg Stanley** - *Initial work* - [Greg Stanley](https://github.com/gregstanley)

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* README.md template from [PurpleBooth](https://gist.githubusercontent.com/PurpleBooth/109311bb0361f32d87a2/raw/8254b53ab8dcb18afc64287aaddd9e5b6059f880/README-Template.md)
* Peter Ellis Jones for this [Generating Legal Chess Moves](https://peterellisjones.com/posts/generating-legal-chess-moves-efficiently/) article that I must have read ten times or more.
