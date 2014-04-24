namespace Microsoft.AspNet.Mvc.Core.Test
{
    // An IAntiForgeryTokenSerializer that can be passed to MoQ.
    public abstract class MockableAntiForgeryTokenSerializer : IAntiForgeryTokenSerializer
    {
        public abstract object Deserialize(string serializedToken);
        public abstract string Serialize(object token);

        AntiForgeryToken IAntiForgeryTokenSerializer.Deserialize(string serializedToken)
        {
            return (AntiForgeryToken)Deserialize(serializedToken);
        }

        string IAntiForgeryTokenSerializer.Serialize(AntiForgeryToken token)
        {
            return Serialize(token);
        }
    }
}