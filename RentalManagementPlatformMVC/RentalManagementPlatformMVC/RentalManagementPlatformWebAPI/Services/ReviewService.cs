using AutoMapper;
using RentalManagementPlatformWebAPI.DTO.RoomList;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories;
using RentalManagementPlatformWebAPI.Repositories.Interfaces;
using RentalManagementPlatformWebAPI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RentalManagementPlatformWebAPI.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public ReviewService(IReviewRepository reviewRepository, IUserRepository userRepository, IMapper mapper)
        {
            _reviewRepository = reviewRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<ReviewDto> CreateReviewAsync(CreateReviewDto createReviewDto, int reviewerId)
        {
            var review = _mapper.Map<Review>(createReviewDto);
            review.ReviewerId = reviewerId;
            review.CreatedAt = DateTime.UtcNow;

            var newReview = await _reviewRepository.AddReviewAsync(review);

            var reviewer = await _userRepository.GetByIdAsync(reviewerId);

            var reviewDto = _mapper.Map<ReviewDto>(newReview);
            reviewDto.ReviewerName = reviewer?.Name ?? "Anonymous";

            return reviewDto;
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByRoomAsync(int roomId)
        {
            var reviews = await _reviewRepository.GetReviewsByRoomIdAsync(roomId);
            var reviewDtos = new List<ReviewDto>();

            foreach (var review in reviews)
            {
                var reviewer = await _userRepository.GetByIdAsync(review.ReviewerId.GetValueOrDefault());
                var reviewDto = _mapper.Map<ReviewDto>(review);
                reviewDto.ReviewerName = reviewer?.Name ?? "Anonymous";
                reviewDtos.Add(reviewDto);
            }

            return reviewDtos;
        }
    }
}
