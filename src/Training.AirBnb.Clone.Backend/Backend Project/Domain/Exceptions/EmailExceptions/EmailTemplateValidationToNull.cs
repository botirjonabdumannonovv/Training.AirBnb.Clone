﻿namespace Backend_Project.Domain.Exceptions.EmailExceptions;

public class EmailTemplateValidationToNull : Exception
{
    public EmailTemplateValidationToNull()
    {

    }
    public EmailTemplateValidationToNull(string message) : base(message)
    {

    }
}