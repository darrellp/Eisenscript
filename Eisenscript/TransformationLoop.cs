namespace Eisenscript;

internal class TransformationLoop
{
    #region Private variables
    private int _repetitions;
    private Transformation _transformation;
    #endregion

    public TransformationLoop(int repetitions, Transformation transformation)
    {
        _repetitions = repetitions;
        _transformation = transformation;
    }
}