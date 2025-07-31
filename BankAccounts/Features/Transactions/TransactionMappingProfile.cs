using AutoMapper;
using BankAccounts.Features.Transactions.DTOs;

namespace BankAccounts.Features.Transactions
{
    /// <summary>
    /// Профиль маппинга для сущности <see cref="Transaction"/>.
    /// Отвечает за преобразование DTO в модель и обратно.
    /// </summary>
    public class TransactionMappingProfile : Profile
    {
        /// <summary>
        /// Профиль маппинга для сущности <see cref="Transaction"/> и связанных DTO.
        /// Используется библиотека AutoMapper для преобразования данных между слоями приложения.
        /// </summary>
        public TransactionMappingProfile()
        {
            CreateMap<TransactionCreateDto, Transaction>()
                .ForMember(dest => dest.Id,
                    option => option.MapFrom(src => Guid.NewGuid())) 
                .ForMember(dest => dest.Timestamp,
                    option => option.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Type,
                    option => option.MapFrom(src => Enum.Parse<TransactionType>(src.Type, true)))
                .ForMember(dest => dest.Account,
                    option => option.Ignore());

            CreateMap<Transaction, TransactionDto>()
                .ForMember(dest => dest.Type,
                    option => option.MapFrom(src => src.Type.ToString()));
        }
    }
}
