using System.Threading.Tasks;
using Api.Models.Dtos;

namespace Api.Services.Interfaces;

public interface IFileService
{
    public Task UploadFiles(UploadFilesDto uploadFilesModel);
    public Task<byte[]> DownloadFiles(DownloadFilesDto downloadFilesModel, int userId);
}