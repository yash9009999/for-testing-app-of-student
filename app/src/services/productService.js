import { BASE_URL } from '../api';

export async function fetchProducts(page = 1, pageSize = 100) {
  try {
    const qs = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
    const response = await fetch(`${BASE_URL}/product?${qs.toString()}`);
    return await response.json();
  } catch (error) {
    console.error('Error fetching products:', error);
    throw error;
  }
}

export async function fetchProductById(id) {
  try {
    const response = await fetch(`${BASE_URL}/product/${id}`);
    return await response.json();
  } catch (error) {
    console.error('Error fetching product by id:', error);
    throw error;
  }
}
