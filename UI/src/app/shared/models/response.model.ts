export class ResponseModel<T = unknown> {
    status: string | number = '';
    message: string = '';
    data: T | null = null;
}

export interface PagedResult<T> {
    items: T[];
    page: number;
    pageSize: number;
    totalCount: number;
    hasMore: boolean;
}

export function isSuccessResponse(response: Pick<ResponseModel, 'status'> | null | undefined): boolean {
    return response?.status === 0 || response?.status === '0';
}
