using System;
using AutoMapper;
using TestTemplate17.Core.Entities;

namespace TestTemplate17.Application.Foos.Queries;

public class FooGetModel
{
    public Guid Id { get; set; }
    public string Text { get; set; }
    public byte[] RowVersion { get; set; }

    public class FooGetModelProfile : Profile
    {
        public FooGetModelProfile()
        {
            CreateMap<Foo, FooGetModel>();
        }
    }
}
