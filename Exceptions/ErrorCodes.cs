namespace VAM.Exceptions
{
    public static class USER_ERROR
    {
        public static readonly ErrorDetails USER_NOT_FOUND = new ErrorDetails("User not found", 404, "USER_NOT_FOUND");
        public static readonly ErrorDetails USER_ALREADY_ACTIVE = new ErrorDetails("User is already active", 400, "USER_ALREADY_ACTIVE");
    }
}
