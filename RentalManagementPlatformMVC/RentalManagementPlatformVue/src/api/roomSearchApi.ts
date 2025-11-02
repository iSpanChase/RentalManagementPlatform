import apiClient from './axiosInstance';

interface RoomListSearchDtoResponse extends Record<string, unknown> {
  room_id?: number;
  title?: string;
  description?: string;
  price_per_night?: number;
  max_guests?: number;
  host_id?: number;
  host_name?: string;
  city_name?: string;
  district_id?: number;
  district_name?: string;
  address_line?: string;
  rating_avg?: number;
  reviews_count?: number;
  cover_bucket?: string;
  cover_object_key?: string;
  cover_content_type?: string;
  created_at?: string;
  updated_at?: string;
  cover_image_url?: string;
  amenities?: string[];
  status?: string;
  is_deleted?: boolean;
  photo_urls?: string[];
}

interface RoomSummaryResponseDtoResponse extends Record<string, unknown> {
  room_id?: number;
  title?: string;
  status?: string;
  host_name?: string;
  main_image_url?: string;
  price_per_night?: number;
  rating_avg?: number;
  city_name?: string;
  district_name?: string;
  address_line?: string;
  is_deleted?: boolean;
  host_id?: number;
  description?: string;
  max_guests?: number;
  photo_urls?: string[];
  reviews_count?: number;
  geo?: GeoLocationDto;
}

interface GeoLocationDto extends Record<string, unknown> {
  lat?: number;
  lng?: number;
  latitude?: number;
  longitude?: number;
}

interface HostDto extends Record<string, unknown> {
  host_name?: string;
  hostName?: string;
}

interface AddressDto extends Record<string, unknown> {
  full_address?: string;
  fullAddress?: string;
}

interface RoomDetailResponseDto extends Record<string, unknown> {
  room_id?: number;
  roomId?: number;
  title?: string;
  description?: string;
  max_guests?: number;
  maxGuests?: number;
  price_per_night?: number;
  pricePerNight?: number;
  status?: string;
  is_deleted?: boolean;
  isDeleted?: boolean;
  host_id?: number;
  hostId?: number;
  city_name?: string;
  cityName?: string;
  district_id?: number;
  districtId?: number;
  district_name?: string;
  districtName?: string;
  address_line?: string;
  addressLine?: string;
  address?: AddressDto;
  host?: HostDto;
  geo?: GeoLocationDto;
  rating_avg?: number;
  ratingAvg?: number;
  reviews_count?: number;
  reviewsCount?: number;
  cover_bucket?: string;
  coverBucket?: string;
  cover_object_key?: string;
  coverObjectKey?: string;
  cover_content_type?: string;
  coverContentType?: string;
  created_at?: string;
  createdAt?: string;
  updated_at?: string;
  updatedAt?: string;
  photo_urls?: string[];
  photoUrls?: string[];
  amenities?: string[];
  main_image_url?: string;
  mainImageUrl?: string;
}

export interface RoomCard {
  roomId: number;
  title: string;
  pricePerNight: number;
  ratingAvg: number;
  cityName?: string;
  districtName?: string;
  addressLine?: string;
  mainImageUrl?: string;
}

export interface GeoLocation {
  lat: number;
  lng: number;
}

export interface RoomPhoto {
  photoId: number;
  url: string;
  sortOrder?: number;
}

export interface RoomDetail {
  roomId: number;
  title: string;
  description?: string;
  pricePerNight: number;
  status?: string;
  isDeleted: boolean;
  maxGuests: number;
  ratingAvg: number;
  reviewsCount: number;
  cityName?: string;
  districtName?: string;
  addressLine?: string;
  fullAddress?: string;
  geo?: GeoLocation;
  hostId?: number;
  hostName?: string;
  mainImageUrl?: string;
  photoUrls: string[];
  photos: RoomPhoto[];
}

const coalesceValue = <T>(
  source: Record<string, unknown> | undefined,
  ...keys: string[]
): T | undefined => {
  if (!source) {
    return undefined;
  }

  for (const key of keys) {
    if (Object.prototype.hasOwnProperty.call(source, key)) {
      const value = source[key];
      if (value !== undefined && value !== null) {
        return value as T;
      }
    }
  }

  return undefined;
};

const normalizePhotoUrls = (rawUrls: string[], mainImage?: string): string[] => {
  const combined = [mainImage, ...rawUrls]
    .filter((url): url is string => typeof url === 'string' && url.length > 0);

  const unique: string[] = [];
  const seen = new Set<string>();

  for (const url of combined) {
    if (!seen.has(url)) {
      seen.add(url);
      unique.push(url);
    }
  }

  return unique;
};

const mapRoomPhotos = (rawPhotos: unknown): RoomPhoto[] => {
  if (!Array.isArray(rawPhotos)) {
    return [];
  }

  const photos: Array<RoomPhoto | null> = rawPhotos.map((item) => {
    if (!item || typeof item !== 'object') {
      return null;
    }

    const data = item as Record<string, unknown>;
    const photoId =
      coalesceValue<number>(data, 'photo_id', 'photoId', 'id') ?? 0;
    const url =
      coalesceValue<string>(data, 'url', 'photo_url', 'photoUrl') ?? '';
    const sortOrder =
      coalesceValue<number>(data, 'sort_order', 'sortOrder');

    if (photoId === 0 || !url) {
      return null;
    }

    const photo: RoomPhoto = {
      photoId,
      url,
    };

    if (typeof sortOrder === 'number') {
      photo.sortOrder = sortOrder;
    }

    return photo;
  });

  return photos
    .filter((photo): photo is RoomPhoto => photo !== null)
    .sort((a, b) => {
      const orderA = a.sortOrder ?? Number.MAX_SAFE_INTEGER;
      const orderB = b.sortOrder ?? Number.MAX_SAFE_INTEGER;
      return orderA - orderB;
    });
};

const mapGeoLocation = (dto?: GeoLocationDto): GeoLocation | undefined => {
  if (!dto) {
    return undefined;
  }

  const lat = coalesceValue<number>(dto, 'lat', 'latitude');
  const lng = coalesceValue<number>(dto, 'lng', 'longitude');

  if (typeof lat === 'number' && typeof lng === 'number') {
    return { lat, lng };
  }

  return undefined;
};

const mapRoomSearchResult = (dto: RoomListSearchDtoResponse): RoomCard => {
  const mainImage = coalesceValue<string>(dto, 'cover_image_url', 'coverImageUrl');
  const rawPhotos = coalesceValue<string[]>(dto, 'photo_urls', 'photoUrls') ?? [];
  const photoUrls = normalizePhotoUrls(rawPhotos, mainImage);
  const imageUrl = photoUrls[0];

  return {
    roomId: coalesceValue<number>(dto, 'room_id', 'roomId') ?? 0,
    title: coalesceValue<string>(dto, 'title') ?? '',
    pricePerNight: coalesceValue<number>(dto, 'price_per_night', 'pricePerNight') ?? 0,
    ratingAvg: coalesceValue<number>(dto, 'rating_avg', 'ratingAvg') ?? 3,
    cityName: coalesceValue<string>(dto, 'city_name', 'cityName'),
    districtName: coalesceValue<string>(dto, 'district_name', 'districtName'),
    addressLine: coalesceValue<string>(dto, 'address_line', 'addressLine'),
    mainImageUrl: imageUrl,
  };
};

const mapRoomSummaryFromCache = (dto: RoomSummaryResponseDtoResponse): RoomCard => mapRoomSummaryToCard(dto);

const mapRoomDetail = (dto: RoomDetailResponseDto): RoomDetail => {
  const photos = mapRoomPhotos(coalesceValue<Record<string, unknown>[]>(dto, 'photos'));
  const rawPhotos = coalesceValue<string[]>(dto, 'photo_urls', 'photoUrls') ?? [];
  const mainImage =
    coalesceValue<string>(dto, 'main_image_url', 'mainImageUrl') ?? photos[0]?.url;
  const combinedPhotoSources = [
    ...photos.map((photo) => photo.url),
    ...rawPhotos,
  ];
  const photoUrls = normalizePhotoUrls(combinedPhotoSources, mainImage);
  const hostDto = coalesceValue<HostDto>(dto, 'host');
  const hostName = coalesceValue<string>(hostDto, 'host_name', 'hostName')
    ?? coalesceValue<string>(dto, 'host_name', 'hostName');
  const addressDto = coalesceValue<AddressDto>(dto, 'address');
  const fullAddress = coalesceValue<string>(addressDto, 'full_address', 'fullAddress');
  const cityName = coalesceValue<string>(dto, 'city_name', 'cityName');
  const districtName = coalesceValue<string>(dto, 'district_name', 'districtName');
  const addressLine = coalesceValue<string>(dto, 'address_line', 'addressLine');
  const fallbackAddress = [cityName, districtName, addressLine].filter((part) => !!part).join('');
  const resolvedAddress = fullAddress ?? (fallbackAddress.length > 0 ? fallbackAddress : undefined);
  const geo = mapGeoLocation(coalesceValue<GeoLocationDto>(dto, 'geo'));

  return {
    roomId: coalesceValue<number>(dto, 'room_id', 'roomId') ?? 0,
    title: coalesceValue<string>(dto, 'title') ?? '',
    description: coalesceValue<string>(dto, 'description'),
    pricePerNight: coalesceValue<number>(dto, 'price_per_night', 'pricePerNight') ?? 0,
    status: coalesceValue<string>(dto, 'status'),
    isDeleted: coalesceValue<boolean>(dto, 'is_deleted', 'isDeleted') ?? false,
    maxGuests: coalesceValue<number>(dto, 'max_guests', 'maxGuests') ?? 0,
    ratingAvg: coalesceValue<number>(dto, 'rating_avg', 'ratingAvg') ?? 0,
    reviewsCount: coalesceValue<number>(dto, 'reviews_count', 'reviewsCount') ?? 0,
    cityName,
    districtName,
    addressLine,
    fullAddress: resolvedAddress,
    geo,
    hostId: coalesceValue<number>(dto, 'host_id', 'hostId'),
    hostName,
    mainImageUrl: photoUrls[0],
    photoUrls,
    photos,
  };
};

const mapRoomDetailFromCache = (dto: RoomSummaryResponseDtoResponse): RoomDetail => {
  const rawPhotos = coalesceValue<string[]>(dto, 'photo_urls', 'photoUrls') ?? [];
  const mainImage = coalesceValue<string>(dto, 'main_image_url', 'mainImageUrl');
  const photoUrls = normalizePhotoUrls(rawPhotos, mainImage);
  const geo = mapGeoLocation(coalesceValue<GeoLocationDto>(dto, 'geo'));
  const cityName = coalesceValue<string>(dto, 'city_name', 'cityName');
  const districtName = coalesceValue<string>(dto, 'district_name', 'districtName');
  const addressLine = coalesceValue<string>(dto, 'address_line', 'addressLine');
  const fallbackAddress = [cityName, districtName, addressLine].filter((part) => !!part).join('');

  return {
    roomId: coalesceValue<number>(dto, 'room_id', 'roomId') ?? 0,
    title: coalesceValue<string>(dto, 'title') ?? '',
    description: coalesceValue<string>(dto, 'description'),
    pricePerNight: coalesceValue<number>(dto, 'price_per_night', 'pricePerNight') ?? 0,
    status: coalesceValue<string>(dto, 'status'),
    isDeleted: coalesceValue<boolean>(dto, 'is_deleted', 'isDeleted') ?? false,
    maxGuests: coalesceValue<number>(dto, 'max_guests', 'maxGuests') ?? 0,
    ratingAvg: coalesceValue<number>(dto, 'rating_avg', 'ratingAvg') ?? 0,
    reviewsCount: coalesceValue<number>(dto, 'reviews_count', 'reviewsCount') ?? 0,
    cityName,
    districtName,
    addressLine,
    fullAddress: fallbackAddress.length > 0 ? fallbackAddress : undefined,
    geo,
    hostId: coalesceValue<number>(dto, 'host_id', 'hostId'),
    hostName: coalesceValue<string>(dto, 'host_name', 'hostName'),
    mainImageUrl: photoUrls[0],
    photoUrls,
    photos: [],
  };
};

export const mapRoomDetailToCard = (detail: RoomDetail): RoomCard => ({
  roomId: detail.roomId,
  title: detail.title,
  pricePerNight: detail.pricePerNight,
  ratingAvg: detail.ratingAvg,
  cityName: detail.cityName,
  districtName: detail.districtName,
  addressLine: detail.addressLine,
  mainImageUrl: detail.mainImageUrl ?? detail.photoUrls[0],
});

const mapRoomSummaryToCard = (dto: RoomSummaryResponseDtoResponse): RoomCard => {
  const detail = mapRoomDetailFromCache(dto);
  return mapRoomDetailToCard(detail);
};

/**
 * Fetches the list of hot rooms from the API.
 */
export const fetchHotRooms = async (): Promise<RoomDetail[]> => {
  const response = await apiClient.get<RoomSummaryResponseDtoResponse[]>('/Rooms/hot');
  return response.data
    .map(mapRoomDetailFromCache)
    .filter((room) => room.roomId !== 0);
};

/**
 * Searches for rooms based on a keyword.
 * @param query The keyword to search for.
 */
export const searchRooms = async (query: string): Promise<RoomCard[]> => {
  if (!query) {
    return [];
  }
  const response = await apiClient.get<RoomListSearchDtoResponse[]>('/Search', {
    params: { query },
  });
  return response.data.map(mapRoomSearchResult);
};

export const fetchRoomDetail = async (roomId: number): Promise<RoomDetail> => {
  const response = await apiClient.get<RoomDetailResponseDto>(`/Rooms/${roomId}`);
  return mapRoomDetail(response.data);
};
