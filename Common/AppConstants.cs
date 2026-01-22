namespace LegacyOrderService.Common;

public static class AppConstants
{
    // ─────────────────────────────
    // Application
    // ─────────────────────────────
    public const string AppName = "rs-legacy-order-service";
    // ─────────────────────────────
    // Database
    // ─────────────────────────────
    public const string DatabaseFileName = "orders.db";
    // ─────────────────────────────
    // Graceful shutdown timeout
    // ─────────────────────────────
    public const int GracefulShutdownTimeoutMilliSecs = 10000;
}