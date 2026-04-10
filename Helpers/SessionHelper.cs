namespace InsuranceClaims.Helpers
{
    /// <summary>
    /// Constant keys used to store/retrieve session values.
    /// </summary>
    public static class SessionKeys
    {
        public const string UserId   = "UserId";
        public const string UserRole = "UserRole";
        public const string UserEmail = "UserEmail";
    }

    /// <summary>
    /// Extension methods on ISession for clean session access.
    /// </summary>
    public static class SessionExtensions
    {
        public static bool IsLoggedIn(this ISession session)
            => session.GetInt32(SessionKeys.UserId).HasValue;

        public static int GetUserId(this ISession session)
            => session.GetInt32(SessionKeys.UserId) ?? 0;

        public static string GetUserRole(this ISession session)
            => session.GetString(SessionKeys.UserRole) ?? string.Empty;

        public static string GetUserEmail(this ISession session)
            => session.GetString(SessionKeys.UserEmail) ?? string.Empty;
    }
}
