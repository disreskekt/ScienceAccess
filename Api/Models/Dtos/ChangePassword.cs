namespace Api.Models.Dtos
{
    public class ChangePassword
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}