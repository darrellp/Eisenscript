namespace Eisenscript;

public class TransformationLoop
{
    public int Reps { get; }

    public Transformation Transform { get; }

    internal TransformationLoop(int repetitions, Transformation transformation)
    {
        Reps = repetitions;
        Transform = transformation;
    }
}