using AutoMapper;
using DigitizingNoteFs.Core.Models;
using DigitizingNoteFs.Core.ViewModels;

namespace DigitizingNoteFs.Core.Mappers
{
    public static class AppMapper
    {
        private static IMapper Mapper { get; }

        static AppMapper()
        {
            Mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MapperProfile>();
            }).CreateMapper();
        }

        public static FsNoteModel ToModel(this FsNoteViewModel viewModel)
        {
            return Mapper.Map<FsNoteModel>(viewModel);
        }

    }
}
