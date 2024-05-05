
using AutoMapper;
using DigitizingNoteFs.Core.Models;
using DigitizingNoteFs.Core.ViewModels;

namespace DigitizingNoteFs.Core.Mappers
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<FsNoteViewModel, FsNoteModel>();
        }
    }
}
