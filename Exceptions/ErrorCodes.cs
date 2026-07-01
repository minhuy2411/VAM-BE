namespace VAM.Exceptions
{
    public static class USER_ERROR
    {
        public static readonly ErrorDetails USER_NOT_FOUND = new ErrorDetails("User not found", 404, "USER_NOT_FOUND");
        public static readonly ErrorDetails USER_ALREADY_ACTIVE = new ErrorDetails("User is already active", 400, "USER_ALREADY_ACTIVE");
    }

    public static class ORDER_ERROR
    {
        public static readonly ErrorDetails ORDER_NOT_FOUND = new ErrorDetails("Order not found", 404, "ORDER_NOT_FOUND");
        public static readonly ErrorDetails ORDER_EMPTY_ITEMS = new ErrorDetails("Order must have at least one item", 400, "ORDER_EMPTY_ITEMS");
        public static readonly ErrorDetails PRODUCT_NOT_FOUND = new ErrorDetails("Product not found", 404, "PRODUCT_NOT_FOUND");
        public static readonly ErrorDetails PRODUCT_NOT_AVAILABLE = new ErrorDetails("Product is not available for purchase", 400, "PRODUCT_NOT_AVAILABLE");
        public static readonly ErrorDetails INSUFFICIENT_STOCK = new ErrorDetails("Insufficient stock", 400, "INSUFFICIENT_STOCK");
        public static readonly ErrorDetails INVALID_STATUS_TRANSITION = new ErrorDetails("Invalid order status transition", 400, "INVALID_STATUS_TRANSITION");
        public static readonly ErrorDetails ORDER_FORBIDDEN = new ErrorDetails("You do not have permission to perform this action", 403, "ORDER_FORBIDDEN");
    }
}
