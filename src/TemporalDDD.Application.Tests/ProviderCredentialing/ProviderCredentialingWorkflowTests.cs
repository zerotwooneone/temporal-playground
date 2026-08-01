using TemporalDDD.Application.ProviderCredentialing;

namespace TemporalDDD.Application.Tests.ProviderCredentialing;

public class ProviderCredentialingWorkflowTests
{
    private static readonly DateOnly FixedExpiryDate = new DateOnly(2027, 1, 1);

    #region CredentialingInput Tests
    [Fact]
    public void CredentialingInput_WhenCreatedWithValidValues_ContainsAllProperties()
    {
        // Arrange
        var providerId = "provider-123";
        var providerPublicId = "pub-456";
        var evaluationPublicId = "eval-789";
        var licenseNumber = "LICENSE123456";
        var medicalBoard = "California";
        var firstName = "John";
        var lastName = "Doe";
        var email = "john.doe@example.com";
        var specialty = "Cardiology";

        // Act
        var input = new CredentialingInput(
            providerId,
            providerPublicId,
            evaluationPublicId,
            licenseNumber,
            medicalBoard,
            FixedExpiryDate,
            firstName,
            lastName,
            email,
            specialty
        );

        // Assert
        input.ProviderId.Should().Be(providerId);
        input.ProviderPublicId.Should().Be(providerPublicId);
        input.EvaluationPublicId.Should().Be(evaluationPublicId);
        input.LicenseNumber.Should().Be(licenseNumber);
        input.MedicalBoard.Should().Be(medicalBoard);
        input.ExpiryDate.Should().Be(FixedExpiryDate);
        input.FirstName.Should().Be(firstName);
        input.LastName.Should().Be(lastName);
        input.Email.Should().Be(email);
        input.Specialty.Should().Be(specialty);
    }
    #endregion

    #region ManualReviewCompletedSignal Tests
    [Fact]
    public void ManualReviewCompletedSignal_WhenApprovedTrue_SetsApprovedProperty()
    {
        // Arrange
        var approved = true;
        var notes = "Approved";

        // Act
        var signal = new ManualReviewCompletedSignal(approved, notes);

        // Assert
        signal.Approved.Should().BeTrue();
        signal.Notes.Should().Be(notes);
    }

    [Fact]
    public void ManualReviewCompletedSignal_WhenApprovedFalse_SetsApprovedProperty()
    {
        // Arrange
        var approved = false;
        var notes = "Missing documentation";

        // Act
        var signal = new ManualReviewCompletedSignal(approved, notes);

        // Assert
        signal.Approved.Should().BeFalse();
        signal.Notes.Should().Be(notes);
    }

    [Fact]
    public void ManualReviewCompletedSignal_WhenNotesIsNull_SetsNotesToNull()
    {
        // Arrange
        var approved = true;
        string? notes = null;

        // Act
        var signal = new ManualReviewCompletedSignal(approved, notes);

        // Assert
        signal.Approved.Should().BeTrue();
        signal.Notes.Should().BeNull();
    }
    #endregion

    #region ApplicationFailedException Tests
    [Fact]
    public void ApplicationFailedException_WhenCreatedWithMessage_SetsMessageProperty()
    {
        // Arrange
        var message = "Workflow failed due to manual review rejection";

        // Act
        var exception = new ApplicationFailedException(message);

        // Assert
        exception.Message.Should().Be(message);
    }
    #endregion
}
