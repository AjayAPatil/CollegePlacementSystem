export class ResponseModel {
    status: string = '';
    message: string = '';
    data: any = null;
}

export interface PagedResult<T> {
    items: T[];
    page: number;
    pageSize: number;
    totalCount: number;
    hasMore: boolean;
}
