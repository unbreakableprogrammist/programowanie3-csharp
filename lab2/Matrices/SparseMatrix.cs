namespace Matrices.Matrix;

public class SparseMatrix : Matrix
{
    private Dictionary<(int row, int column), int> _data;

    public SparseMatrix(int rows, int columns) : base(rows, columns)
    {
        _data = new Dictionary<(int row, int column), int>();
    }

    public override float this[int row, int column]
    {
        get
        {
            if (row < 0 || row >= Rows || column < 0 || column >= Columns) 
                throw new ArgumentException("Index out of range.");
            return _data.GetValueOrDefault((row, column));
        }
        set
        {
            if (row < 0 || row >= Rows || column < 0 || column >= Columns) 
                throw new ArgumentException("Index out of range.");
            if ( value == 0.0f )
                {
                _data.Remove((row, column));
                return;
                }
                _data[(row, column)] =  value;
        }
    }
    
}