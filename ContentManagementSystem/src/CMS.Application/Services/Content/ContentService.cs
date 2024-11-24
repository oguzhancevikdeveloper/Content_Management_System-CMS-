﻿using CMS.Domain.DTOs.Content;
using CMS.Domain.Models.Content;
using CMS.Domain.Models.User;
using CMS.Domain.Repositories.Category;
using CMS.Domain.Repositories.Content;
using CMS.Domain.Repositories.User;
using CMS.Domain.Services.Content;
using CMS.Shared.DTOs;
using Mapster;
using Microsoft.AspNetCore.Http;

namespace CMS.Application.Services.Content;

public class ContentService : IContentService
{
    private readonly IContentRepository _contentRepository;
    private readonly IUserContentRepository _userContentRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ContentService(IContentRepository contentRepository, IUserContentRepository userContentRepository, IUserRepository userRepository, ICategoryRepository categoryRepository)
    {
        _contentRepository = contentRepository;
        _userContentRepository = userContentRepository;
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<Response<NoDataDto>> AddContentAsync(Guid userId, ContentDto contentDto)
    {
        var content = contentDto.Adapt<Domain.Models.Content.Content>();
        var contentVariants = contentDto.contentVariantDtos.Adapt<List<ContentVariant>>();

        content.Variants = contentVariants;
        foreach (var item in contentVariants)
        {
            item.ContentId = content.Id;
        }

        await _contentRepository.AddContentAsync(content);

        Domain.Models.User.User user = await _userRepository.GetUserByIdAsync(userId);
        var userContent = new UserContent
        {
            ContentId = content.Id,
            UserId = user.Id
        };

        await _userContentRepository.AddUserContentAsync(userContent);

        return Response<NoDataDto>.Success(StatusCodes.Status201Created);
    }

    public async Task<Response<NoDataDto>> DeleteContentAsync(Guid contentId, Guid userId)
    {
        var content = await _userContentRepository.GetUserContentByIdsAsync(contentId: contentId, userId: userId);

        if (content == null) return Response<NoDataDto>.Fail("Content not found", StatusCodes.Status404NotFound, true);


        await _userContentRepository.DeleteUserContentAsync(userId: userId, contentId: contentId);

        return Response<NoDataDto>.Success(StatusCodes.Status200OK);
    }

    public async Task<Response<IEnumerable<ContentDto>>> GetAllContentsAsync()
    {
        var contents = await _contentRepository.GetAllContentsAsync();
        var contentDtos = contents.Adapt<IEnumerable<ContentDto>>();

        return Response<IEnumerable<ContentDto>>.Success(contentDtos, StatusCodes.Status200OK);

    }

    public async Task<Response<ContentDto>> GetContentByIdAsync(Guid contentId)
    {
        var content = await _contentRepository.GetContentByIdAsync(contentId);
        if (content == null) return Response<ContentDto>.Fail("Content not found", StatusCodes.Status404NotFound, true);
        var contentDto = content.Adapt<ContentDto>();

        return Response<ContentDto>.Success(contentDto, StatusCodes.Status200OK);

    }

    public async Task<Response<ContentDto>> GetContentByTitleAsync(string title)
    {
        var content = await _contentRepository.GetContentByTitleAsync(title);
        if (content == null) return Response<ContentDto>.Fail("Content not found", StatusCodes.Status404NotFound, true);
        var contentDto = content.Adapt<ContentDto>();

        return Response<ContentDto>.Success(contentDto, StatusCodes.Status200OK);

    }

    public async Task<Response<IEnumerable<ContentDto>>> GetContentsByCategoryAsync(Guid categoryId)
    {
        var contents = await _contentRepository.GetContentsByCategoryAsync(categoryId);
        if (!contents.Any()) return Response<IEnumerable<ContentDto>>.Fail("No contents found for the specified category", StatusCodes.Status404NotFound, true);
        var contentDtos = contents.Adapt<IEnumerable<ContentDto>>();

        return Response<IEnumerable<ContentDto>>.Success(contentDtos, StatusCodes.Status200OK);

    }

    public async Task<Response<IEnumerable<ContentDto>>> GetContentsByLanguageAsync(string language)
    {
        var contents = await _contentRepository.GetContentsByLanguageAsync(language);
        if (!contents.Any()) return Response<IEnumerable<ContentDto>>.Fail("No contents found for the specified language", StatusCodes.Status404NotFound, true);
        var contentDtos = contents.Adapt<IEnumerable<ContentDto>>();

        return Response<IEnumerable<ContentDto>>.Success(contentDtos, StatusCodes.Status200OK);

    }

    public async Task<Response<IEnumerable<ContentVariantDto>>> GetContentVariantsAsync(Guid contentId)
    {
        var content = await _contentRepository.GetContentVariantsAsync(contentId);
        if (!content.Any()) return Response<IEnumerable<ContentVariantDto>>.Fail("Content variants not found", StatusCodes.Status404NotFound, true);
        var contentVariantDtos = content.Adapt<IEnumerable<ContentVariantDto>>();

        return Response<IEnumerable<ContentVariantDto>>.Success(contentVariantDtos, StatusCodes.Status200OK);
    }

    public async Task<Response<NoDataDto>> UpdateContentAsync(Guid userId, Guid contentId, ContentDto contentDto)
    {
        var categoryIsExist = await _categoryRepository.GetCategoryByIdAsync(contentDto.CategoryId);
        if (categoryIsExist == null) return Response<NoDataDto>.Fail("Category not found", StatusCodes.Status404NotFound, true);

        var userContentIsExist = await _userContentRepository.GetUserContentByIdsAsync(userId, contentId);
        if (userContentIsExist == null) return Response<NoDataDto>.Fail("User Content not found", StatusCodes.Status404NotFound, true);

        var existingContent = await _contentRepository.GetContentByIdAsync(contentId);
        if (existingContent == null) return Response<NoDataDto>.Fail("Content not found", StatusCodes.Status404NotFound, true);

        contentDto.Adapt(existingContent);

        foreach (var item in existingContent.Variants)
        {
            item.Content = existingContent;
            item.ContentId = existingContent.Id;
        }


        await _contentRepository.UpdateContentAsync(existingContent);

        return Response<NoDataDto>.Success(StatusCodes.Status200OK);
    }
}
