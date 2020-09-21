using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Dynamitey;
using SpecFlow.Assist.Dynamic.PropertyNameFormatting;

namespace TechTalk.SpecFlow.Assist
{
  public static class DynamicTableHelpers
  {
    private const string ERRORMESS_PROPERTY_DIFF_SET = "Properties differs between the table and the set";
    private const string ERRORMESS_INSTANCETABLE_FORMAT = "Can only create instances of tables with one row, or exactly 2 columns and several rows";
    private const string ERRORMESS_NOT_ON_TABLE = "The '{0}' value not present in the table, but on the instance";
    private const string ERRORMESS_NOT_ON_INSTANCE = "The '{0}' value not present on the instance, but in the table";
    private const string ERRORMESS_VALUE_DIFFERS =
        "The '{0}' value differs from table and instance.\n\tInstance:\t'{1}'(type: {2}).\n\tTable:\t\t'{3}'(type: {4})";

    private const string ERRORMESS_NUMBER_OF_ROWS_DIFFERS =
        "Number of rows for table ({0} rows) and set ({1} rows) differs";

    private const string ERRORMESS_SET_VALUES_DIFFERS =
        "A difference was found on row '{0}' for column '{1}' (property '{2}').\n\tInstance:\t'{3}'(type: {4}).\n\tTable:\t\t'{5}'(type: {6})";

    private static readonly IPropertyNameFormatter defaultPropertyNameFormatter = new DefaultPropertyNameFormatter();

    /// <summary>
    /// Create a dynamic object from the headers and values of the <paramref name="table"/>
    /// </summary>
    /// <param name="table">the table to create a dynamic object from</param>
    /// <param name="doTypeConversion">should types be converted according to conventions described in https://github.com/marcusoftnet/SpecFlow.Assist.Dynamic/wiki/Conversion-conventions#property-type-conversions</param>
    /// <returns>the created object</returns>
    public static ExpandoObject CreateDynamicInstance(this Table table, bool doTypeConversion = true)
    {
        return CreateDynamicInstance(table, defaultPropertyNameFormatter, doTypeConversion);
    }

    /// <summary>
    /// Create a dynamic object from the headers and values of the <paramref name="table"/>
    /// </summary>
    /// <param name="table">the table to create a dynamic object from</param>
    /// <param name="propertyNameFormatter">The formatter to use to format property names</param>
    /// <param name="doTypeConversion">should types be converted according to conventions described in https://github.com/marcusoftnet/SpecFlow.Assist.Dynamic/wiki/Conversion-conventions#property-type-conversions</param>
    /// <returns>the created object</returns>
    public static ExpandoObject CreateDynamicInstance(
        this Table table,
        IPropertyNameFormatter propertyNameFormatter,
        bool doTypeConversion = true)
    {
        if (table.Header.Count == 2 && table.RowCount > 1)
        {
            var horizontalTable = CreateHorizontalTable(table);
            return CreateDynamicInstance(horizontalTable.Rows[0], propertyNameFormatter, doTypeConversion);
        }

        if (table.RowCount == 1)
        {
            return CreateDynamicInstance(table.Rows[0], propertyNameFormatter, doTypeConversion);
        }

        throw new DynamicInstanceFromTableException(ERRORMESS_INSTANCETABLE_FORMAT);
    }

    /// <summary>
    /// Creates a set of dynamic objects based of the <paramref name="table"/> headers and values
    /// </summary>
    /// <param name="table">the table to create a set of dynamics from</param>
    /// <param name="doTypeConversion">should types be converted according to conventions described in https://github.com/marcusoftnet/SpecFlow.Assist.Dynamic/wiki/Conversion-conventions#property-type-conversions</param>
    /// <returns>a set of dynamics</returns>
    public static IEnumerable<dynamic> CreateDynamicSet(this Table table, bool doTypeConversion = true)
    {
        return CreateDynamicSet(table, defaultPropertyNameFormatter, doTypeConversion);
    }

    /// <summary>
    /// Creates a set of dynamic objects based of the <paramref name="table"/> headers and values
    /// </summary>
    /// <param name="table">the table to create a set of dynamics from</param>
    /// <param name="propertyNameFormatter">The formatter to use to format property names</param>
    /// <param name="doTypeConversion">should types be converted according to conventions described in https://github.com/marcusoftnet/SpecFlow.Assist.Dynamic/wiki/Conversion-conventions#property-type-conversions</param>
    /// <returns>a set of dynamics</returns>
    public static IEnumerable<dynamic> CreateDynamicSet(
        this Table table,
        IPropertyNameFormatter propertyNameFormatter,
        bool doTypeConversion = true)
    {
        return from r in table.Rows
            select CreateDynamicInstance(r, propertyNameFormatter, doTypeConversion);
    }

    /// <summary>
    /// Validates if a dynamic instance <paramref name="instance"/> matches the <paramref name="table"/>
    /// Throws descriptive exception if not
    /// </summary>
    /// <param name="table">the table to compare the instance against</param>
    /// <param name="instance">the instance to compare the table against</param>
    /// <param name="doTypeConversion">should types be converted according to conventions described in https://github.com/marcusoftnet/SpecFlow.Assist.Dynamic/wiki/Conversion-conventions#property-type-conversions</param>
    public static void CompareToDynamicInstance(this Table table, dynamic instance, bool doTypeConversion = true)
    {
      CompareToDynamicInstance(table, instance, defaultPropertyNameFormatter, doTypeConversion);
    }

    /// <summary>
    /// Validates if a dynamic instance <paramref name="instance"/> matches the <paramref name="table"/>
    /// Throws descriptive exception if not
    /// </summary>
    /// <param name="table">the table to compare the instance against</param>
    /// <param name="instance">the instance to compare the table against</param>
    /// <param name="propertyNameFormatter">The formatter to use to format property names</param>
    /// <param name="doTypeConversion">should types be converted according to conventions described in https://github.com/marcusoftnet/SpecFlow.Assist.Dynamic/wiki/Conversion-conventions#property-type-conversions</param>
    public static void CompareToDynamicInstance(
        this Table table,
        dynamic instance,
        IPropertyNameFormatter propertyNameFormatter,
        bool doTypeConversion = true)
    {
        IList<string> propDiffs = GetPropertyDifferences(table, instance, propertyNameFormatter);
        if (propDiffs.Any())
            throw new DynamicInstanceComparisonException(propDiffs);

        AssertValuesOfRowDifference(table.Rows[0], instance, propertyNameFormatter, doTypeConversion);
    }

    /// <summary>
    /// Validates that the dynamic set <paramref name="set"/> matches the <paramref name="table"/>
    /// Throws descriptive exception if not
    /// </summary>
    /// <param name="table">the table to compare the set against</param>
    /// <param name="set">the set to compare the table against</param>
    /// <param name="doTypeConversion">should types be converted according to conventions described in https://github.com/marcusoftnet/SpecFlow.Assist.Dynamic/wiki/Conversion-conventions#property-type-conversions</param>
    public static void CompareToDynamicSet(this Table table, IList<dynamic> set, bool doTypeConversion = true)
    {
        CompareToDynamicSet(table, set, defaultPropertyNameFormatter, doTypeConversion);
    }

    /// <summary>
    /// Validates that the dynamic set <paramref name="set"/> matches the <paramref name="table"/>
    /// Throws descriptive exception if not
    /// </summary>
    /// <param name="table">the table to compare the set against</param>
    /// <param name="set">the set to compare the table against</param>
    /// <param name="propertyNameFormatter">The formatter to use to format property names</param>
    /// <param name="doTypeConversion">should types be converted according to conventions described in https://github.com/marcusoftnet/SpecFlow.Assist.Dynamic/wiki/Conversion-conventions#property-type-conversions</param>
    public static void CompareToDynamicSet(
        this Table table,
        IList<dynamic> set,
        IPropertyNameFormatter propertyNameFormatter,
        bool doTypeConversion = true)
    {
        AssertEqualNumberOfRows(table, set);

        IList<string> propDiffs = GetPropertyDifferences(table, set[0], propertyNameFormatter);
        if (propDiffs.Any())
        {
            throw new DynamicSetComparisonException(ERRORMESS_PROPERTY_DIFF_SET, propDiffs);
        }

        // Now we know that the table and the list has the same number of rows and properties

        var valueDifference = GetSetValueDifferences(table, set, propertyNameFormatter, doTypeConversion);

        if (valueDifference.Any())
        {
            throw new DynamicSetComparisonException(ERRORMESS_PROPERTY_DIFF_SET, valueDifference);
        }
    }

    private static List<string> GetSetValueDifferences(
        Table table,
        IList<object> set,
        IPropertyNameFormatter propertyNameFormatter,
        bool doTypeConversion = true)
    {
      var memberNames = Dynamic.GetMemberNames(set[0]);
      var valueDifference = new List<string>();

      for (var i = 0; i < set.Count; i++)
      {
        foreach (var memberName in memberNames)
        {
          var currentHeader = string.Empty;
          var rowValue = GetRowValue(i, table, memberName, propertyNameFormatter, out currentHeader, doTypeConversion);
          var rowType = rowValue.GetType().Name;
          var instanceValue = Dynamic.InvokeGet(set[i], memberName);
          var instanceType = instanceValue.GetType().Name;

          if (!instanceValue.Equals(rowValue))
          {
            var difference = string.Format(ERRORMESS_SET_VALUES_DIFFERS,
                                           i + 1,
                                           currentHeader,
                                           memberName,
                                           instanceValue,
                                           instanceType,
                                           rowValue,
                                           rowType);

            valueDifference.Add(difference);
          }
        }
      }
      return valueDifference;
    }

    private static object GetRowValue(
        int rowIndex,
        Table table,
        string memberName,
        IPropertyNameFormatter propertyNameFormatter,
        out string currentHeader,
        bool doTypeConversion = true)
    {
      object rowValue = null;
      currentHeader = string.Empty;
      foreach (var header in table.Header)
      {
        if (propertyNameFormatter.Format(header) == memberName)
        {
          currentHeader = header;
          rowValue = CreateTypedValue(table.Rows[rowIndex][header], doTypeConversion);
          break;
        }
      }
      return rowValue;
    }

    private static void AssertValuesOfRowDifference(
        TableRow tableRow,
        dynamic instance,
        IPropertyNameFormatter propertyNameFormatter,
        bool doTypeConversion = true)
    {
      IList<string> valueDiffs = ValidateValuesOfRow(tableRow, instance, propertyNameFormatter, doTypeConversion);
      if (valueDiffs.Any())
        throw new DynamicInstanceComparisonException(valueDiffs);
    }

    private static IList<string> GetPropertyDifferences(
        Table table,
        dynamic instance,
        IPropertyNameFormatter propertyNameFormatter)
    {
      var tableHeadersAsPropertyNames = table.Header.Select(propertyNameFormatter.Format);
      IEnumerable<string> instanceMembers = Dynamic.GetMemberNames(instance);

      return GetPropertyNameDifferences(tableHeadersAsPropertyNames, instanceMembers);
    }

    private static void AssertEqualNumberOfRows(Table table, IList<object> set)
    {
      if (table.RowCount != set.Count)
      {
        var mess = string.Format(ERRORMESS_NUMBER_OF_ROWS_DIFFERS, table.RowCount, set.Count);
        throw new DynamicSetComparisonException(mess);
      }
    }

    private static IList<string> ValidateValuesOfRow(
        TableRow tableRow,
        dynamic instance,
        IPropertyNameFormatter propertyNameFormatter,
        bool doTypeConversion = true)
    {
      var valueDiffs = new List<string>();

      foreach (var header in tableRow.Keys)
      {
        var propertyName = propertyNameFormatter.Format(header);
        var valueFromInstance = Dynamic.InvokeGet(instance, propertyName);
        var typeFromInstance = valueFromInstance.GetType().Name;
        var valueFromTable = CreateTypedValue(tableRow[header], doTypeConversion);
        var typeFromTable = valueFromTable.GetType().Name;

        if (!valueFromInstance.Equals(valueFromTable))
        {
          var mess = string.Format(ERRORMESS_VALUE_DIFFERS, propertyName, valueFromInstance, typeFromInstance, valueFromTable, typeFromTable);
          valueDiffs.Add(mess);
        }
      }
      return valueDiffs;
    }

    private static IList<string> GetPropertyNameDifferences(IEnumerable<string> tableHeadersAsPropertyNames, IEnumerable<string> instanceMembers)
    {
      var allMembersInTableButNotInInstance = tableHeadersAsPropertyNames.Except(instanceMembers);
      var allMembersInInstanceButNotInTable = instanceMembers.Except(tableHeadersAsPropertyNames);

      var diffs = new List<string>();

      diffs.AddRange(
          allMembersInInstanceButNotInTable.Select(
              m => string.Format(ERRORMESS_NOT_ON_TABLE, m)));

      diffs.AddRange(
          allMembersInTableButNotInInstance.Select(
              m => string.Format(ERRORMESS_NOT_ON_INSTANCE, m)));

      return diffs;
    }

    private static Table CreateHorizontalTable(Table verticalFieldValueTable)
    {
      var dic = verticalFieldValueTable.
                      Rows.ToDictionary(row => row[0], row => row[1]);

      var horizontalTable = new Table(dic.Keys.ToArray());
      horizontalTable.AddRow(dic);
      return horizontalTable;
    }

    private static ExpandoObject CreateDynamicInstance(
        TableRow tablerow,
        IPropertyNameFormatter propertyNameFormatter,
        bool doTypeConversion = true)
    {
      dynamic expando = new ExpandoObject();
      var dicExpando = expando as IDictionary<string, object>;

      foreach (var header in tablerow.Keys)
      {
        var propName = propertyNameFormatter.Format(header);
        var propValue = CreateTypedValue(tablerow[header], doTypeConversion);
        dicExpando.Add(propName, propValue);
      }

      return expando;
    }

    private static object CreateTypedValue(string valueFromTable, bool doTypeConversion = true)
    {
      if (!doTypeConversion)
        return valueFromTable;

      int i;
      if (int.TryParse(valueFromTable, out i))
        return i;

      double db;
      if (Double.TryParse(valueFromTable, out db))
      {
        decimal d;
        if (Decimal.TryParse(valueFromTable, out d) && d.Equals((decimal)db))
        {
          return db;
        }
        return d;
      }

      bool b;
      if (Boolean.TryParse(valueFromTable, out b))
        return b;

      DateTime dt;
      if (DateTime.TryParse(valueFromTable, out dt))
        return dt;

      return valueFromTable;
    }
  }
}
