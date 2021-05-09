LINES OF ACTION + AI - MADE BY TA"P
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

this project is my master project to achieve a degree of technician
i used unity in this project and therefore used c#, currently there are 3 gamemodes - human vs human, AI vs human, AI vs AI
when choosing the AI vs human option you can choose whice color to play as below the tick boxes

for game rules visit - https://www.boardspace.net/loa/english/index.html

this implementation of ai used negascout (an improvement of negamax and a partial transposition table (discarding alpha beta saving for better runtime))
there is a partial implementation of killer moves hueristic but since transposition table can do similiar actions i discarded it

the base game uses a bit board to check for winning game states and legal moves, this is also the key for the transposition table (to string of the white pieces bit board and the black pieces bit board)

base game also uses counting arrays to determine amount of pieces in each direction of the board (4 arrays for line, row, and both diagonals), updating amount each move (6 changes when no eating is made, 4 when eating a piece)

in addition added a dictionary to the model so that pieces can be removed and added to board in O(1) using the position of the piece on the board

model class is for all logical base game methods and data structures, game class is for graphical methods (view), AI class is for all ai methods

SPECS
-----
Visual Studio 2019 community version 16.9.2
Unity version 2020.3.4f2




