namespace Snowflake.EntityFrameworkCore.Migrations.Internal;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
/// Helper class that stores the column dependencies in a specific table.
/// Usually a column has dependencies when it is a computed column.
/// </summary>
public class SnowflakeColumnGraph
{
    private readonly IDictionary<string, IList<IColumn>> _edges;
    
    internal SnowflakeColumnGraph(ITable table)
    {
        this._edges = new Dictionary<string, IList<IColumn>>();
        
        foreach (var column in table.Columns)
        {
            if (string.IsNullOrEmpty(column.ComputedColumnSql))
            {
                continue;
            }

            var referencedColumns = table.Columns
                .Where(referencedColumn => !referencedColumn.Name.Equals(column.Name))
                .Where(referencedColumn => column.ComputedColumnSql.Contains(referencedColumn.Name))
                .ToList();

            this._edges.Add(column.Name, referencedColumns);
        }
    }

    internal IEnumerable<IColumn> TopologicalSort(ITable table)
    {
        var visited = new HashSet<string>();
        var sortedColumns = new List<IColumn>();
        foreach (var column in table.Columns)
        {
            this.TopologicalSort(column, visited, sortedColumns);
        }
        
        return sortedColumns;
    }
    
    private void TopologicalSort(IColumn column, ISet<string> visited, IList<IColumn> sortedColumns)
    {
        if (visited.Contains(column.Name))
        {
            return;
        }

        visited.Add(column.Name);
        
        if (this._edges.TryGetValue(column.Name, out var dependencies)) 
        {
            foreach (var dependency in dependencies) 
            {
                this.TopologicalSort(dependency, visited, sortedColumns);
            }
        }
        
        sortedColumns.Add(column);
    }
}