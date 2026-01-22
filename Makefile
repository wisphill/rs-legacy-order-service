ARCH ?= osx-arm64

# Run all unit tests in the tests project
# Usage: make test
test:
	dotnet test tests/LegacyOrderService.Tests.csproj
	
# Run specific test by fully qualified name in the tests project
# Usage: make test-specific TEST=Namespace.ClassName.MethodName
# sample: make test-specific  TEST=LegacyOrderService.Services.OrderServiceTests.CreateOrder_ValidProduct_SavesOrder
test-specific:
	dotnet test tests/LegacyOrderService.Tests.csproj --filter "FullyQualifiedName~$(TEST)"

# Build the project
# Usage: make build
build:
	dotnet build -c Release

build-arch:
	dotnet build --runtime $(ARCH) -c Release

debug:
	dotnet run -c Debug
	
# Run the project
# Usage: make run
run:
	dotnet run -c Release

publish:
	rm -rf ./publish
	dotnet publish -c Release -o ./publish --self-contained true

publish-arch:
	rm -rf ./publish
	dotnet publish -c Release -o ./publish --self-contained true -r $(ARCH)

clean:
    rm -rf bin obj publish
	dotnet clean