using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WorkOps.Data;
using WorkOps.Models;
using WorkOps.Models.Errors;
using WorkOps.Utils;

namespace WorkOps.Services;

public class InputModelService(ApplicationDbContext context,
    ILogger<InputModelService> logger)
{
    /// <summary>
    /// DBコンテキスト
    /// </summary>
    private readonly ApplicationDbContext _context = context;

    /// <summary>
    /// DBから入力モデルリストを取得する
    /// </summary>
    /// <typeparam name="TInputModel">入力モデル</typeparam>
    /// <param name="entities">DB</param>
    /// <returns>入力モデルリスト</returns>
    public async Task<(IQueryable<TInputModel>, IQueryable<TEntity>)>
        GetInputModelsAsync<TInputModel, TEntity>()
        where TInputModel : BaseInputModel, new()
        where TEntity : BaseEntity
    {
        var entities = _context.Set<TEntity>()
            .Include(m => m.CreatedUser)
            .Include(m => m.UpdatedUser)
            ;
        var models = entities.AsEnumerable()
            .Select(e => new TInputModel
            {
                Id = e.Id,
                CreatedUserName = e.CreatedUser?.FullName
                    ?? e.CreatedBy ?? string.Empty,
                UpdatedUserName = e.UpdatedUser?.FullName
                    ?? e.UpdatedBy ?? string.Empty
            })
            .ToList()
            ;
        foreach (var model in models)
        {
            var entity = await _context.Set<TEntity>()
                .FirstOrDefaultAsync(e => e.Id == model.Id);
            PropertyUtil.CopyProperties(entity, model);
        }
        logger.LogDebug("GetInputModelsAsync: Count={Count}",
            models.Count);
        return (models.AsQueryable(), entities);
    }

    /// <summary>
    /// IDからDBを検索し入力モデルを取得する
    /// </summary>
    /// <typeparam name="TInputModel">入力モデル</typeparam>
    /// <param name="id">ID</param>
    /// <returns>入力モデル</returns>
    public async Task<(TInputModel?, TEntity?)>
        GetInputModelAsync<TInputModel, TEntity>(int id)
        where TInputModel : BaseInputModel, new()
        where TEntity : BaseEntity
    {
        var entity = await _context.Set<TEntity>()
            .Include(m => m.CreatedUser)
            .Include(m => m.UpdatedUser)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (entity == null)
        {
            return (null, null);
        }
        var inputModel = new TInputModel
        {
            CreatedUserName = entity.CreatedUser?.FullName
                ?? entity.CreatedBy ?? string.Empty,
            UpdatedUserName = entity.UpdatedUser?.FullName
                ?? entity.UpdatedBy ?? string.Empty
        };
        PropertyUtil.CopyProperties(entity, inputModel);
        logger!.LogDebug("GetInputModelAsync: Id={Id}, TEntity={TEntity}",
            id, typeof(TEntity).Name);
        return (inputModel, entity);
    }

    /// <summary>
    /// 入力モデルをエンティティに設定する
    /// </summary>
    /// <typeparam name="TInputModel"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="inputModel"></param>
    /// <param name="id"></param>
    /// <returns>エンティティ</returns>
    public async Task<TEntity?> InputModelToEntity<TInputModel, TEntity>(
        TInputModel inputModel, int id = 0)
        where TInputModel : BaseInputModel
        where TEntity : BaseEntity, new()
    {
        var entity = id == 0
            ? new TEntity()
            : await _context.Set<TEntity>().FirstOrDefaultAsync(m => m.Id == id);
        if (entity == null)
        {
            return null;
        }
        PropertyUtil.CopyProperties(inputModel, entity);
        return entity;
    }

    /// <summary>
    /// エンティティをDBコンテキストに登録または更新する
    /// （DB保存はしないのでこの後SaveChangesAsyncが必要）
    /// </summary>
    /// <typeparam name="TEntity">エンティティ</typeparam>
    /// <param name="entity">エンティティ</param>
    /// <param name="isSave">保存するか？</param>
    /// <returns></returns>
    public async Task SaveChangesAsync<TEntity>(TEntity entity,
        bool isSave = true)
        where TEntity : BaseEntity
    {
        if (entity.Id == 0)
        {
            _context.Set<TEntity>().Add(entity);
        }
        else
        {
            _context.Set<TEntity>().Update(entity);
        }
        if (isSave)
        {
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// DBに保存する
    /// </summary>
    /// <typeparam name="TInputModel">入力モデル</typeparam>
    /// <typeparam name="TEntity">DBエンティティ</typeparam>
    /// <param name="inputModel">入力モデル</param>
    /// <param name="id">ID</param>
    /// <param name="isCheckDuplicate">重複チェックをするか？</param>
    /// <returns></returns>
    public async Task<ErrorCode> SaveInputModelAsync<TInputModel, TEntity>(
        TInputModel inputModel, int id = 0, bool isCheckDuplicate = true,
        Dictionary<string, object>? conditions = null)
        where TInputModel : BaseInputModel
        where TEntity : BaseEntity, new()
    {
        if (isCheckDuplicate
            && await inputModel.IsDuplicateAsync<TEntity>(_context, conditions))
        {
            // 名前重複エラー
            return ErrorCode.Duplicate;
        }
        var entity = await InputModelToEntity<TInputModel, TEntity>(
            inputModel, id);
        if (entity == null)
        {
            return ErrorCode.NotFound;
        }
        await SaveChangesAsync(entity);
        logger!.LogInformation("SaveInputModelAsync: Id={Id} TEntity={TEntity}",
            entity.Id, typeof(TEntity).Name);
        inputModel.Id = entity.Id;
        return ErrorCode.None;
    }

    /// <summary>
    /// DBから削除する
    /// </summary>
    /// <typeparam name="TEntity">DBエンティティ</typeparam>
    /// <param name="id">ID</param>
    /// <returns></returns>
    public async Task DeleteInputModelAsync<TEntity>(
        int id)
        where TEntity : BaseEntity
    {
        var entity = await _context.Set<TEntity>()
            .FirstOrDefaultAsync(m => m.Id == id);
        if (entity == null)
        {
            return;
        }
        _context.Set<TEntity>().Remove(entity!);
        logger!.LogInformation("DeleteInputModelAsync: Id={Id} TEntity={TEntity}",
            id, typeof(TEntity).Name);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// DBから削除する
    /// </summary>
    /// <typeparam name="TEntity">DBエンティティ</typeparam>
    /// <typeparam name="TChildEntity">子エンティティ</typeparam>
    /// <param name="id">ID</param>
    /// <returns></returns>
    public async Task<ErrorCode> DeleteInputModelAsync<TEntity, TChildEntity>(
        int id)
        where TEntity : BaseEntity
        where TChildEntity : BaseEntity
    {
        var entity = await _context.Set<TEntity>()
            .FirstOrDefaultAsync(m => m.Id == id);
        if (entity == null)
        {
            return ErrorCode.None;
        }

        if (await HasChildEntitiesAsync<TEntity, TChildEntity>(entity!))
        {
            // 子エンティティが存在する場合は削除しない
            return ErrorCode.HasChildren;
        }

        _context.Set<TEntity>().Remove(entity!);
        logger!.LogInformation("DeleteInputModelAsync: Id={Id} TEntity={TEntity}",
            id, typeof(TEntity).Name);
        await _context.SaveChangesAsync();
        return ErrorCode.None;
    }

    /// <summary>
    /// DBから削除する
    /// </summary>
    /// <typeparam name="TEntity">DBエンティティ</typeparam>
    /// <typeparam name="TChildEntity">子エンティティ</typeparam>
    /// <typeparam name="TChildEntity2">子エンティティ2</typeparam>
    /// <param name="id">ID</param>
    /// <returns></returns>
    public async Task<ErrorCode> DeleteInputModelAsync
        <TEntity, TChildEntity, TChildEntity2>(
        int id)
        where TEntity : BaseEntity
        where TChildEntity : BaseEntity
        where TChildEntity2 : BaseEntity    
    {
        var entity = await _context.Set<TEntity>()
            .FirstOrDefaultAsync(m => m.Id == id);
        if (entity == null)
        {
            return ErrorCode.None;
        }

        if (await HasChildEntitiesAsync<TEntity, TChildEntity>(entity!)
            || await HasChildEntitiesAsync<TEntity, TChildEntity2>(entity!))
        {
            // 子エンティティが存在する場合は削除しない
            return ErrorCode.HasChildren;
        }

        _context.Set<TEntity>().Remove(entity!);
        logger!.LogInformation("DeleteInputModelAsync: Id={Id} TEntity={TEntity}",
            entity.Id, typeof(TEntity).Name);
        await _context.SaveChangesAsync();
        return ErrorCode.None;
    }

    /// <summary>
    /// 子エンティティが存在するか？
    /// </summary>
    /// <typeparam name="TEntity">親エンティティ</typeparam>
    /// <typeparam name="TChildEntity">子エンティティ</typeparam>
    /// <param name="entity">親エンティティ</param>
    /// <returns></returns>
    private async Task<bool> HasChildEntitiesAsync<TEntity, TChildEntity>(
        TEntity entity)
        where TChildEntity : BaseEntity
    {
        var parentEntry = _context.Entry(entity!);
        var parentType = _context.Model.FindEntityType(typeof(TEntity))!;
        // TChild に対する FK を取得
        var fk = parentType.GetReferencingForeignKeys()
            .SingleOrDefault(f =>
                f.DeclaringEntityType.ClrType == typeof(TChildEntity) &&
                f.DeleteBehavior == DeleteBehavior.Cascade);
        if (fk != null)
        {
            IQueryable<TChildEntity> query = _context.Set<TChildEntity>();
            // FK条件を動的に構築（列名非依存）
            for (int i = 0; i < fk.Properties.Count; i++)
            {
                // 子（TChild）の FK
                var fkProp = fk.Properties[i];
                // 親（TParent）の PK
                var pkProp = fk.PrincipalKey.Properties[i];
                var value = parentEntry.Property(pkProp.Name).CurrentValue;
                if (value == null)
                {
                    continue;
                }
                query = WhereDynamic(query, fkProp.Name, value);
            }
            return await query.AnyAsync();
        }
        return false;
    }

    /// <summary>
    /// Where句を動的に構築する
    /// </summary>
    /// <typeparam name="T">クエリの型</typeparam>
    /// <param name="source">クエリ</param>
    /// <param name="propertyName">プロパティ名</param>
    /// <param name="value">値</param>
    /// <returns>Where句</returns>
    private static IQueryable<T> WhereDynamic<T>(
        IQueryable<T> source,
        string propertyName,
        object value)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, propertyName);
        var constant = Expression.Constant(value);
        var equal = Expression.Equal(property, constant);
        var lambda = Expression.Lambda<Func<T, bool>>(equal, parameter);

        return source.Where(lambda);
    }
}
