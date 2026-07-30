using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.ProviderCredentialing.ValueObjects;

public sealed record MedicalBoard
{
    private static readonly HashSet<string> AllowedBoards = new(StringComparer.OrdinalIgnoreCase)
    {
        "American Medical Association",
        "American Board of Medical Specialties",
        "American Osteopathic Association",
        "National Board of Medical Examiners",
        "Medical Board of California",
        "Texas Medical Board",
        "Florida Board of Medicine",
        "New York State Medical Board",
        "General Medical Council",
        "Medical Council of India",
        "Australian Health Practitioner Regulation Agency"
    };

    public string Value { get; }

    private MedicalBoard(string value)
    {
        Value = value;
    }

    public static Result<MedicalBoard> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<MedicalBoard>.Failure("Medical board cannot be null or whitespace");

        var trimmed = value.Trim();

        if (trimmed.Length < 2 || trimmed.Length > 150)
            return Result<MedicalBoard>.Failure("Medical board must be between 2 and 150 characters");

        if (!AllowedBoards.Contains(trimmed))
            return Result<MedicalBoard>.Failure($"Medical board '{trimmed}' is not recognized. Allowed boards: {string.Join(", ", AllowedBoards)}");

        return Result<MedicalBoard>.Success(new MedicalBoard(trimmed));
    }

    public static implicit operator string(MedicalBoard medicalBoard) => medicalBoard.Value;

    public override string ToString() => Value;
}
