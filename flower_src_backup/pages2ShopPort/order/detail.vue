<template>
	<view class="page">
		<view class="pageStatus">
			{{ storeOrder.statusName[info.status] }}
		</view>
		<view class="square">
			<view class="header">
				<view>
					<image :src="data.goodsImgList[0]" mode="aspectFill"></image>
				</view>
				<view class="texts">
					<view class="title">{{ info.relatedDemand }}</view>
				</view>
			</view>
			<view class="prices">
				<view class="content">
					<view>发布时间：{{ info.createTime }}</view>
					<view>收货时间：{{ info.receivingTime }}</view>
				</view>
				<view class="price">￥{{ info.price.toFixed(2) }}</view>
			</view>
		</view>
		<view class="square" v-if="info.status == 2">
			<view class="title">已有{{ data.applyCount }}名用户申请接单</view>
		</view>
		<view class="square">
			<view class="title">{{ data.shopName }}</view>
			<view class="content">
				<view>联系方式：{{ data.shopMobile }}</view>
				<view>详情地址：{{ data.shopProvince + data.shopCity + data.shopArea + data.shopAddress }}</view>
			</view>
		</view>
		<view class="square">
			<view class="imgs">
				<image v-for="item, index in data.goodsImgList" :key="item" :src="item" mode="aspectFill"
					@click="previewImages(data.goodsImgList, index)"></image>
			</view>
		</view>
		<view class="square">
			<view class="list">
				<view class="item" v-if="info.shopRefundAmount">
					<view class="label">花店退款金额</view>
					<view class="value price">￥{{ info.shopRefundAmount }}
					</view>
				</view>
				<view class="item" v-if="info.userRefundAmount">
					<view class="label">用户退款金额</view>
					<view class="value price">￥{{ info.userRefundAmount }}
					</view>
				</view>
				<view class="item" v-if="data.userName">
					<view class="label">接单用户</view>
					<view class="value">{{ data.userName }}
					</view>
				</view>
				<view class="item" v-if="data.userMobile">
					<view class="label">用户联系方式</view>
					<view class="value">{{ data.userMobile }}
					</view>
				</view>
				<view class="item">
					<view class="label">收货方式</view>
					<view class="value">{{ data.receivingTypeName }}
					</view>
				</view>
				<view class="item" v-if="[4, 5].includes(info.status)">
					<view class="label">发货类型</view>
					<view class="value">{{ info.deliveryType == 0 ? '物流' : '自提' }}
					</view>
				</view>
				<view class="item" v-if="info.deliveryType === 0 && info.deliveryTime">
					<view class="label">发货时间</view>
					<view class="value">{{ info.deliveryTime }}
					</view>
				</view>
				<view class="item" v-if="info.expressName">
					<view class="label">快递名称</view>
					<view class="value">{{ info.expressName }}
					</view>
				</view>
				<view class="item" v-if="info.logisticsNo">
					<view class="label">物流单号</view>
					<view class="value">{{ info.logisticsNo }}
					</view>
				</view>
				<view class="item">
					<view class="label">收货地址</view>
					<view class="value">{{ data.shopProvince + data.shopCity + data.shopArea + data.shopAddress }}
					</view>
				</view>
				<view class="item">
					<view class="label">订单编号</view>
					<view class="value">{{ info.orderNo }}</view>
				</view>
				<view class="item">
					<view class="label">订单金额</view>
					<view class="value price">￥{{ info.price.toFixed(2) }}</view>
				</view>
			</view>
		</view>

		<view class="square">
			<view class="content">{{ info.remarks }}</view>
		</view>
		<view class="square" v-if="[3, 4].includes(info.status)">
			<view class="red">用户发货后7天会自动确认收货 , 请注意查收相关信息!!</view>
		</view>
		<view style="height: 100rpx;"></view>
		<view class="btns">
			<view class="right" v-if="info.status == 2"
				@click="href('/pages2ShopPort/order/accept?orderNo=' + info.orderNo)">选择接单用户</view>
		</view>
	</view>
</template>

<script>
export default {
	data() {
		return {
			id: "",
			data: {
				goodsImgList: []
			},
			info: {
				price: 0
			},
		};
	},
	onLoad(options) {
		this.id = options.id
		this.OrderDetail()
	},
	methods: {
		async OrderDetail() {
			let res = await this.request('Order/TaskDetails', {
				queryId: this.id
			})
			this.data = res.data
			this.info = res.data.info
		},
		previewImages(urls, index) {
			uni.previewImage({
				urls, // 图片路径数组
				current: index // 当前显示的图片索引
			});
		},
		// 确认收货
		orderReceiving() {
			const fun = () => {
				this.request('Order/OrderReceiving', {
					orderNo: this.order.orderNo
				}).then(res => {
					this.toast("确认收货成功")
					setTimeout(() => {
						uni.navigateBack()
					}, 400)
				})
			}
			fun()
		},
		// 去申请退款
		toRefund(item) {
			this.href(`/pagesShop/order/refund?id=` + item.Id)
		},
		// 去物流
		toLogistics(id) {
			this.href(`/pagesShop/order/logistics?id=${id}`)
		}
	}
}
</script>

<style lang="scss" scoped>
.page {
	padding: 32rpx;
}

.pageStatus {
	font-weight: 600;
	font-size: 28rpx;
	color: #8C4FFF;
	padding-bottom: 0;
}

.square {
	margin-top: 24rpx;
	background: #FFFFFF;
	border-radius: 16rpx;
	padding: 24rpx;

	.red {
		color: #DE4343;
	}

	.header {
		display: flex;

		image {
			border-radius: 8rpx;
			width: 132rpx;
			height: 132rpx;
		}

		.texts {
			margin-left: 16rpx;

			.title {
				font-size: 28rpx;
				color: #000000;
				overflow: hidden;
				text-overflow: ellipsis;
				display: -webkit-box;
				-webkit-box-orient: vertical;
				-webkit-line-clamp: 4;
			}

			.content {
				margin-top: 16rpx;
				font-size: 24rpx;
				color: #666666;
			}
		}
	}

	.prices {
		display: flex;
		justify-content: space-between;

		.price {
			font-weight: 600;
			font-size: 40rpx;
			color: #DE4343;
			display: flex;
			align-items: flex-end;
		}
	}

	.content {
		font-size: 24rpx;
		color: #666666;

		>view {
			margin-top: 16rpx;
		}
	}

	.imgs {
		display: flex;
		flex-wrap: wrap;

		image {
			width: 140rpx;
			height: 140rpx;
			margin-right: 12rpx;
			margin-bottom: 24rpx;
		}
	}

	.list {
		.item {
			width: 100%;
			display: flex;
			justify-content: space-between;
			align-items: center;
			padding: 21rpx;

			.label {
				font-size: 26rpx;
				color: #333333;
			}

			.value {
				font-size: 26rpx;
				color: #333333;
			}

			.price {
				color: #DE4343;
			}
		}
	}
}

.btns {
	padding: 16rpx 32rpx;
	position: fixed;
	left: 0;
	bottom: 0;
	width: 750rpx;
	background: #FFFFFF;
	box-shadow: 0rpx 0rpx 16rpx 1rpx rgba(0, 123, 191, 0.2);
	display: flex;
	justify-content: space-between;

	>view {
		width: 100%;
		border-radius: 16rpx;
		padding: 20rpx 0;
		display: flex;
		align-items: center;
		justify-content: center;
	}

	>view:not(:first-child) {
		margin-left: 24rpx;
	}

	.left {
		font-size: 28rpx;
		color: #111111;
		background: #FFFFFF;
		border: 2rpx solid #111111;
	}

	.right {
		background: #8C4FFF;
		font-size: 28rpx;
		color: #FFFFFF;
	}
}
</style>