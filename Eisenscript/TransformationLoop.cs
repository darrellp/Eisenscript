namespace Eisenscript;

internal class TransformationLoop
{
    internal int Reps { get; }

    internal Transformation Transform { get; }

    public TransformationLoop(int repetitions, Transformation transformation)
    {
        Reps = repetitions;
        Transform = transformation;
    }
}