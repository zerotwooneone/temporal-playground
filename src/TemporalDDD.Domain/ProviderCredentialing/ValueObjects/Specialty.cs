namespace TemporalDDD.Domain.ProviderCredentialing.ValueObjects;

public sealed record Specialty
{
    public string Value { get; }

    private Specialty(string value)
    {
        Value = value;
    }

    public static readonly Specialty Anesthesiology = new("Anesthesiology");
    public static readonly Specialty Cardiology = new("Cardiology");
    public static readonly Specialty Dermatology = new("Dermatology");
    public static readonly Specialty EmergencyMedicine = new("Emergency Medicine");
    public static readonly Specialty FamilyMedicine = new("Family Medicine");
    public static readonly Specialty InternalMedicine = new("Internal Medicine");
    public static readonly Specialty Neurology = new("Neurology");
    public static readonly Specialty ObstetricsGynecology = new("Obstetrics & Gynecology");
    public static readonly Specialty Orthopedics = new("Orthopedics");
    public static readonly Specialty Pediatrics = new("Pediatrics");
    public static readonly Specialty Psychiatry = new("Psychiatry");
    public static readonly Specialty Radiology = new("Radiology");
    public static readonly Specialty Surgery = new("Surgery");
    public static readonly Specialty Urology = new("Urology");

    private static readonly HashSet<Specialty> AllSpecialties = new()
    {
        Anesthesiology, Cardiology, Dermatology, EmergencyMedicine, FamilyMedicine,
        InternalMedicine, Neurology, ObstetricsGynecology, Orthopedics, Pediatrics,
        Psychiatry, Radiology, Surgery, Urology
    };

    public static Specialty Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Specialty cannot be null or whitespace", nameof(value));

        var trimmed = value.Trim();

        var specialty = AllSpecialties.FirstOrDefault(s => 
            s.Value.Equals(trimmed, StringComparison.OrdinalIgnoreCase));

        if (specialty == null)
            throw new ArgumentException($"Specialty '{trimmed}' is not recognized. Valid specialties: {string.Join(", ", AllSpecialties.Select(s => s.Value))}", nameof(value));

        return specialty;
    }

    public static implicit operator string(Specialty specialty) => specialty.Value;

    public override string ToString() => Value;
}
