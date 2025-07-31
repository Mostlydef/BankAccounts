using AutoMapper;
using BankAccounts.Features.Accounts.DTOs;

namespace BankAccounts.Features.Accounts
{
    /// <summary>
    /// Профиль AutoMapper для маппинга между DTO и сущностями аккаунтов.
    /// </summary>
    public class AccountMappingProfile : Profile
    {
        /// <summary>
        /// Инициализирует новый экземпляр <see cref="AccountMappingProfile"/>,
        /// настраивая правила преобразования между <see cref="AccountCreateDto"/> и <see cref="Account"/>,
        /// а также между <see cref="Account"/> и <see cref="AccountDto"/>.
        /// </summary>
        public AccountMappingProfile()
        {
            CreateMap<AccountCreateDto, Account>()
                .ForMember(dest => dest.Id,
                    option => option.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.Type,
                    option => option.MapFrom(src => Enum.Parse<AccountType>(src.AccountType, true)))
                .ForMember(dest => dest.OpenDate,
                    option => option.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Balance,
                    option => option.MapFrom(src => 0m))
                .ForMember(dest => dest.CloseDate,
                    option => option.Ignore())
                .ForMember(dest => dest.Transactions,
                    option => option.Ignore());

            CreateMap<Account, AccountDto>()
                .ForMember(dest => dest.Type,
                    option => option.MapFrom(src => src.Type.ToString()));
        }
}
}
