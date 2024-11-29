namespace FargateAPIApp.Shared.Models
{
    public interface IMapper<TModel, TDto>
        where TModel : IModel
        where TDto : class
    {
        bool TryMapToModel(TDto dto, out TModel model);
        bool TryMapToDto(TModel model, out TDto dto);
    }

}
