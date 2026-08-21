using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using static AlmaVault.Models.ViewModel.MentorshipRequestsVM;

namespace AlmaVault.Models.ViewModel
{
    public class MentorshipDetailsVM
    {
        // Request Summary
        public Guid RequestId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime RequestedDate { get; set; } = DateTime.UtcNow;

        // Mentee Details mapped using your MentorCardViewModel
        public MentorCardViewModel MenteeCard { get; set; } = new MentorCardViewModel();

        // Forms for Modals
        public TaskItemVM TaskItem { get; set; } = new TaskItemVM();
        public NoteInputModel NewNote { get; set; } = new NoteInputModel();
    }


    public class NoteInputModel
    {
        [Required(ErrorMessage = "Please select a PDF file to upload.")]
        public IFormFile? PdfFile { get; set; }
    }
}