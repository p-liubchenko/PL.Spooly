namespace Pricer.DAL.Repositories;

[Obsolete("Use Pricer.ICrudRepository<TEntity, TKey> from the domain project.", error: false)]
public interface ICrudRepository<TEntity, in TKey> : Pricer.ICrudRepository<TEntity, TKey>
	where TEntity : class
{
}
