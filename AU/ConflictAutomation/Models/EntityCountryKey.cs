namespace ConflictAutomation.Models
{
    public class EntityCountryKey
    {
        public string EntityWithoutLegalExt { get; set; }
        public string Country { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is EntityCountryKey other)
            {
                return string.Equals(EntityWithoutLegalExt, other.EntityWithoutLegalExt, StringComparison.OrdinalIgnoreCase)
                       && string.Equals(Country, other.Country, StringComparison.Ordinal);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hashEntity = EntityWithoutLegalExt?.ToLowerInvariant().GetHashCode() ?? 0;
            int hashCountry = Country?.GetHashCode() ?? 0;
            return hashEntity ^ hashCountry;
        }
    }
    public class EntityCountryKeyEqualityComparer : IEqualityComparer<EntityCountryKey>
    {
        public bool Equals(EntityCountryKey x, EntityCountryKey y)
        {
            return string.Equals(x.EntityWithoutLegalExt, y.EntityWithoutLegalExt, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(x.Country, y.Country, StringComparison.Ordinal);
        }

        public int GetHashCode(EntityCountryKey obj)
        {
            int hashEntity = obj.EntityWithoutLegalExt?.ToLowerInvariant().GetHashCode() ?? 0;
            int hashCountry = obj.Country?.GetHashCode() ?? 0;
            return hashEntity ^ hashCountry;
        }
    }
}
