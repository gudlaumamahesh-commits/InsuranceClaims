namespace InsuranceClaims.Enums
{
    public enum ClaimStatus
    {
        REGISTERED,
        UNDER_REVIEW,
        APPROVED,
        REJECTED
    }

    public enum VerificationStatus
    {
        PENDING,
        VERIFIED,
        REJECTED
    }

    public enum RiskFlag
    {
        LOW,
        MEDIUM,
        HIGH
    }

    public enum SettlementStatus
    {
        PENDING,
        PROCESSED,
        FAILED
    }

    public enum UserRole
    {
        Customer,
        Officer,
        Surveyor,
        Admin
    }
}
