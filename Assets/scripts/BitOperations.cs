using UnityEngine;

public class BitBoard
{
    // ----------------------------------- Variables -----------------------------------------------
    
    // Bit board of the places i check in my check win method
    public long checkedthis { get; set; }
    // The board 
    long board { get; set; }
    // Whites position
    long whites { get; set; }
    // Blacks position
    long blacks { get; set; }
    // Array for the directions that i can move in
    Vector2Int[] directions = new Vector2Int[9];

    
    // ----------------------------------- Constructors and initializers --------------------------------------------


    // Init all bit boards and possible direction for movement
    public BitBoard()
    {
        board =  0b0111_1110_1000_0001_1000_0001_1000_0001_1000_0001_1000_0001_1000_0001_0111_1110;
        whites = 0b0000_0000_1000_0001_1000_0001_1000_0001_1000_0001_1000_0001_1000_0001_0000_0000;
        blacks = 0b0111_1110_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0111_1110;

        InitDirections();
    }

    // Copy ctor
    public BitBoard(BitBoard b)
    {
        this.blacks = b.blacks;
        this.whites = b.whites;
        this.board = b.board;
        this.checkedthis = b.checkedthis;
        InitDirections();
    }

    // Initialize the directions array
    private void InitDirections() 
    {
        directions[0] = new Vector2Int(0, 0);
        directions[1] = new Vector2Int(1, 0);
        directions[2] = new Vector2Int(0, 8);
        directions[3] = new Vector2Int(1, 8);
        directions[4] = new Vector2Int(-1, 0);
        directions[5] = new Vector2Int(0, -8);
        directions[6] = new Vector2Int(-1, -8);
        directions[7] = new Vector2Int(-1, 8);
        directions[8] = new Vector2Int(1, -8);
    }

    // Init a bit board to save all places i have searched already 
    public void InitCheckedThis()
    {
        checkedthis = 0b0;
    }


    // ----------------------------------------- Getters and setters ------------------------------------------------


    // Return a board of the empty positions in the board
    public long GetEmptyPositions()
    {
        return (board ^ 1) & 0;
    }


    // Get a bool and return that players bitboard for his pieces
    public long GetCurrentPlayersBoard(bool player)
    {
        if (player)
        {
            return whites;
        }
        else
        {
            return blacks;
        }
    }



    // --------------------------------------------- Core Methods ---------------------------------------------------


    // Get an index and a player
    // Check for the number of adjecent piece to that index 
    public int FindLines(int index, bool player)
    {
        long beenthere = checkedthis;
        int length = 0;
        int next = index;
        // add current move into the bitboard for moves made
        checkedthis = (beenthere | TurnIndexToBitBoard(index));
        // If the current index points to nothing
        if ((beenthere ^ checkedthis) == 0) { return 0; }
        length++;
        foreach (Vector2Int i in directions)
        {
            if (InRange(next, i)) { continue; }
            // Dont go back
            if (i == directions[0]) { continue; }
            if ((GetCurrentPlayersBoard(player) & TurnIndexToBitBoard(next + i.x + i.y)) != 0)
            {
                length += FindLines(next + i.x + i.y, player);
            }
        }
        return length;
    }

    // Get the index to eat and the player thats eating
    // Update the correct board by removing a bit
    public void EatingPiece(int eat, bool player) 
    {
        if (player)
        {
            blacks ^= TurnIndexToBitBoard(eat);
        }
        else 
        {
            whites ^= TurnIndexToBitBoard(eat);
        }
    }

    // Get an index in the board as a single line and make a mask with a bit at that index
    public long TurnIndexToBitBoard(int index)
    {
        // long mask = 2^index 
        long mask = 1;
        mask <<= 63 - index; 
        return mask;
    }

    // Get a move
    // Update the bitboard by removning the attacked piece's bit and moving the piece that movesx
    public void MakeMove(Move move)
    {
        // Starting index 
        int before = PositionToIndex(move.pieceToMove.position);
        // Ending index
        int after = PositionToIndex(move.moveto);
        // Update correct players board
        if (move.pieceToMove.player)
        {
            whites |= TurnIndexToBitBoard(after);
            whites ^= TurnIndexToBitBoard(before);
        }
        else
        {
            blacks |= TurnIndexToBitBoard(after);
            blacks ^= TurnIndexToBitBoard(before);
        }
        // Handle eating moves
        if (IsEnemy(move.moveto , move.pieceToMove.player)) 
        {
            EatingPiece(after, move.pieceToMove.player);
        }
        // Update the board with the new bit boards
        board = whites | blacks;
    }

    // Get a move 
    // Undo the effects of a move made on the bitboard
    public void undoMove(Move move)
    {
        // Starting position
        Vector2Int start = move.pieceToMove.position;
        // Ending position 
        Vector2Int end = move.moveto;
        // Current player
        bool player = move.pieceToMove.player;
        if (player)
        {
            // Update whites board
            whites |= TurnIndexToBitBoard(PositionToIndex(start));
            whites ^= TurnIndexToBitBoard(PositionToIndex(end));
            if (move.attack)
            {
                blacks |= TurnIndexToBitBoard(PositionToIndex(end));
            }
        }
        else 
        {
            // Update whites board
            blacks |= TurnIndexToBitBoard(PositionToIndex(start));
            blacks ^= TurnIndexToBitBoard(PositionToIndex(end));
            if (move.attack)
            {
                whites |= TurnIndexToBitBoard(PositionToIndex(end));
            }
        }
        board = whites | blacks;
    }


    // ----------------------------------------------- Utilities --------------------------------------------------


    // Check if this index is in range and can be moved in said direction
    public bool InRange(int index, Vector2Int i)
    {
        // First and last statement checks if im in range of 0 to 64
        // Second checks if im at the start of a line and moving backwards (if yes then stop)
        // Third statement checks if im at the end of a line and moving forward  
        return (index + i.x + i.y > 64) || ((index % 8) == 0 && i.x < 0) ||
               ((index + 1) % 8 == 0 && i.x == 1) || (index + i.x + i.y < 0);
    }

    // Get a bool for a player and check if the position given is pointing to the other players pieces
    public bool IsEnemy(Vector2Int pos, bool player)
    {
        // If true check for black pieces if false check for white pieces
        if (player)
        {
            return IsBlackPiece(pos);
        }
        else
        {
            return IsWhitePiece(pos);
        }
    }

    // Check if a given index is holding anykind of piece
    public bool IsPieceHere(Vector2Int v)
    {
        return (board & TurnIndexToBitBoard(PositionToIndex(v))) != 0;
    }

    // Check if a given index is holding a black piece
    public bool IsBlackPiece(Vector2Int position)
    {
        return (blacks & TurnIndexToBitBoard(PositionToIndex(position))) != 0;
    }

    // Check if a given index is holding a white piece
    public bool IsWhitePiece(Vector2Int position)
    {
        return (whites & TurnIndexToBitBoard(PositionToIndex(position))) != 0;
    }

    // Get a position
    // Turn it into an index that can be used on the bit boards 
    public int PositionToIndex(Vector2Int pos)
    {
        return pos.x + pos.y * 8;
    }

    // Get a starting position, an end position and who is the current player
    // Check if an enemy is present between these 2 points
    public bool IsEnemyBeforeHere(Vector2Int start, Vector2Int end, Vector2Int dir, bool player)
    {
        // Mask to hold the 2 points im checking between
        long mask = 0;

        // Maybe Change this later since u want efficent shit
        // Currently this is O(6)
        // Add bits and check after with single if statement
        while (start != end && IsOnBoard(start.x, start.y))
        {
            start.x += dir.x;
            start.y += dir.y;
            mask |= TurnIndexToBitBoard(PositionToIndex(start));
        }
        mask ^= TurnIndexToBitBoard(PositionToIndex(start));
        // Get the oponnents board
        // Mask holds "on" bits between the 2 positions given
        // if & operation with the black mask returns 0 it means nothing is between these 2 bits - this means no enemy here
        if ((GetCurrentPlayersBoard(!player) & mask) == 0)
        {
            return false;
        }
        // Enemy is present return true 
        return true;
    }

    // Check if a position is on board
    public bool IsOnBoard(int x, int y)
    {
        return (x < 8 && x >= 0) && (y < 8 && y >= 0);
    }

}