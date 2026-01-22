# Run all unit tests
# Usage: make test
test:
	dotnet test
	
# Run specific test by fully qualified name
# Usage: make test-specific TEST=Namespace.ClassName.MethodName
# sample: make test-specific  TEST=LegacyOrderService.Services.OrderServiceTests.CreateOrder_ValidProduct_SavesOrder
test-specific:
	dotnet test --filter "FullyQualifiedName~$(TEST)"
