namespace HelpDesk.Domain.SharedKernel.Primitives
{
    public abstract class ValueObject
    {
        protected abstract IEnumerable<object?> GetEqualityComponents();

        public override bool Equals(object? obj)
        {
            if (obj is null || obj.GetType() != GetType())
                return false;

            var other = (ValueObject)obj;
            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;

                foreach (var component in GetEqualityComponents())
                    hash = (hash * 31) + (component?.GetHashCode() ?? 0);

                return hash;
            }
        }
    }
}
