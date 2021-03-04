using UnityEngine;

public class BitBoard
{
    long checkedthis;
    long board;
    long whites;
    long blacks;
    Vector2Int[] directions = new Vector2Int[9];
    // Init all bit boards and possible direction for movement
    public BitBoard()
    {
        board = 0b0111_1110_1000_0001_1000_0001_1000_0001_1000_0001_1000_0001_1000_0001_0111_1110;
        whites = 0b0000_0000_1000_0001_1000_0001_1000_0001_1000_0001_1000_0001_1000_0001_0000_0000;
        blacks = 0b0111_1110_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0111_1110;

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
    // Get an index and a player and check for the number of adjecent piece to that index 
    public int FindLines(int index, bool player)
    {
        long beenthere = Getcheckedthis();
        int length = 0;
        int next = index;
        // add current move into the bitboard for moves made
        SetCheckedThis(beenthere | TurnIndexToBitBoard(index));
        // If the current index points to nothing
        if ((beenthere ^ Getcheckedthis()) == 0) { return 0; }
        length++;
        foreach (Vector2Int i in GetDirections())
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

    // Check if this index is in range and can be moved in said direction
    public bool InRange(int index, Vector2Int i)
    {
        return (index + i.x + i.y > 64) || (((index + 1) % 8) == 0 && i.x < 0) ||
               ((index + 2) % 8 == 0 && i.x == 1) || (index + i.x + i.y < 0);
    }

    // Get a bool for a player and check if the position given is pointing to the other players pieces
    public bool IsEnemy(Vector2Int pos, bool player) 
    {
        // If true check for black pieces if false check for white pieces
        if (player)
        {
            return IsBlackPiece(pos.x, pos.y);
        }
        else 
        {
            return IsWhitePiece(pos.x, pos.y);
        }
    }

    public void SetPlayersBoard(int eat, bool player) 
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

    public Vector2Int[] GetDirections()
    {
        return directions;
    }
    // Get an index in the board as a single line and make a mask with a bit at that index
    public long TurnIndexToBitBoard(int index)
    {
        // long mask = 2^index 
        long mask = 0b100000000000000000000000000000000000000000000000000000000000000;
        mask >>= index;
        return mask;
    }

    public long Getcheckedthis()
    {
        return checkedthis;
    }
    // Init a bit board to save all places i have searched already 
    public void InitCheckedThis()
    {
        checkedthis = 0b0;
    }

    public void SetBitBoard(long newboard)
    {
        this.board = newboard;
    }
    public void SetCheckedThis(long setchecked)
    {
        this.checkedthis = setchecked;
    }
    // Add an index to the current bit board and return a copy of the new board
    public long MakeTempBoard(int index)
    {
        return GetBitBoard() | TurnIndexToBitBoard(index);
    }

    public long GetBitBoard()
    {
        return board;
    }

    public long GetWhites()
    {
        return whites;
    }

    // Get a starting index, an end index and the player making a move, remove the bit at the start index and add a bit at the end index
    public void MakeMove(Vector2Int start, Vector2Int end, bool player)
    {
        int before = PositionToIndex(start);
        int after = PositionToIndex(end);
        if (player)
        {
            whites |= TurnIndexToBitBoard(before);
            whites ^= TurnIndexToBitBoard(after);
        }
        else
        {
            blacks |= TurnIndexToBitBoard(before);
            blacks ^= TurnIndexToBitBoard(after);
        }
        if (IsEnemy(end ,player)) 
        {
            SetPlayersBoard(after, player);
        }
        board = whites | blacks;
    }

    public void undomove() 
    {
        
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

    public long GetBlacks()
    {
        return blacks;
    }

    public long GetEmptyPositions()
    {
        return (board ^ 1) & 0;
    }

    // Check if a given index is holding anykind of piece
    public bool IsPieceHere(int x, int y)
    {
        return (board & TurnIndexToBitBoard(x + y * 8 - 1)) != 0;
    }

    // Check if a given index is holding a black piece
    public bool IsBlackPiece(int x, int y)
    {
        return (blacks & TurnIndexToBitBoard(x + y * 8 - 1)) != 0;
    }

    // Check if a given index is holding a white piece
    public bool IsWhitePiece(int x, int y)
    {
        return (whites & TurnIndexToBitBoard(x + y * 8 - 1)) != 0;
    }

    public int PositionToIndex(Vector2Int pos) 
    {
        return pos.x + pos.y * 8 - 1;
    }
}