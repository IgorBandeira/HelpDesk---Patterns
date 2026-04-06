using System;
using HelpDesk.Domain.ServiceCatalog.Events;
using HelpDesk.Domain.ServiceCatalog.ValueObjects;
using HelpDesk.Domain.SharedKernel.Primitives;

namespace HelpDesk.Domain.ServiceCatalog.Aggregates
{
    public sealed class Category : AggregateRoot<int>
    {
        public CategoryName Name { get; private set; } = default!;
        public int? ParentId { get; private set; }

        private Category()
        {
        }

        private Category(
            int id,
            CategoryName name,
            int? parentId) : base(id)
        {
            Name = name;
            ParentId = parentId;
        }

        public static Category CreateNew(
            CategoryName name,
            int? parentId)
        {
            return new Category(
                id: 0,
                name: name,
                parentId: parentId);
        }

        public static Category Rehydrate(
            int id,
            CategoryName name,
            int? parentId)
        {
            return new Category(
                id: id,
                name: name,
                parentId: parentId);
        }

        public void RaiseCreatedEvent(string actorUserName, DateTime now)
        {
            Raise(CategoryCreatedDomainEvent.Create(
                categoryId: Id,
                name: Name.Value,
                parentId: ParentId,
                actorUserName: actorUserName,
                occurredAt: now));
        }

        public void RaiseDeletedEvent(string actorUserName, DateTime now)
        {
            Raise(CategoryDeletedDomainEvent.Create(
                categoryId: Id,
                name: Name.Value,
                parentId: ParentId,
                actorUserName: actorUserName,
                occurredAt: now));
        }
    }
}