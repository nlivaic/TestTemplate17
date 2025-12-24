using System.Collections.Generic;
using TestTemplate17.Core.Entities;
using TestTemplate17.Data;

namespace TestTemplate17.Api.Tests.Helpers;

public static class Seeder
{
    public static void Seed(this TestTemplate17DbContext ctx)
    {
        ctx.Foos.AddRange(
            new List<Foo>
            {
                new ("Text 1"),
                new ("Text 2"),
                new ("Text 3")
            });
        ctx.SaveChanges();
    }
}
