using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using TestTemplate17.Common.Exceptions;
using TestTemplate17.Common.Interfaces;
using TestTemplate17.Core.Entities;

namespace TestTemplate17.Application.Foos.Queries;

public class GetFooQuery : IRequest<FooGetModel>
{
    public Guid Id { get; set; }

    private class GetFooQueryHandler : IRequestHandler<GetFooQuery, FooGetModel>
    {
        private readonly IMapper _mapper;
        private readonly IRepository<Foo> _repository;

        public GetFooQueryHandler(IMapper mapper, IRepository<Foo> repository)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public async Task<FooGetModel> Handle(GetFooQuery request, CancellationToken cancellationToken)
        {
            var foo = await _repository.GetSingleAsync(f => f.Id == request.Id);
            if (foo is null)
            {
                throw new EntityNotFoundException(nameof(Foo), request.Id);
            }
            return _mapper.Map<FooGetModel>(foo);
        }
    }
}
