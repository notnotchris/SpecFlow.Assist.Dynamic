Feature: PropertyNameFormatting
	As a developer
	I want the property names of my dynamic code to be formatted as specified
	So that I can perform comparisons

Scenario: Property names are left untouched when preserving formatting
	When I create a dynamic instance with preserved property names from this table
		| This is my (Name) | age |
		| Chris             | 37  |
	Then the 'This is my (Name)' property should equal 'Chris'
		And the age property should equal 37

Scenario: Property names are formatted as expected when using a custom formatter
	When I create a dynamic instance from this table using the 'CustomPropertyNameFormatter' property name formatter
		| Customer ID |
		| C123        |
	Then the 'CustomerID' property should equal 'C123'
