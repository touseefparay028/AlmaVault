using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AlmaVault.ViewModels
{
    public class NoteItemViewModel
    {
        public Guid Id { get; set; }
        public string? Content { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? FilePath { get; set; }
        public string? OriginalFileName { get; set; }
        public bool HasAttachment => !string.IsNullOrEmpty(FilePath);
    }

    public class PostNoteInputModel
    {
        public Guid RequestId { get; set; }
        
        public IFormFile? PdfFile { get; set; }
    }
}