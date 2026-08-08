<template>
	<view>
		<share></share>
		<view class="searchBox">
			<up-search placeholder="搜索商品" v-model="query.queryName" :showAction="false" :clearabled="false"
				@search="getList(true)"></up-search>
		</view>
		<view class="list">
			<view class="item" v-for="item in list" :key="item.Id"
				@click="href(`/pagesShop/task/detail?id=${item.Id}`)">
				<view class="header">
					<image :src="item.goodsImgList[0]" mode="aspectFill"></image>
					<view class="name">
						<view class="text">
							{{ item.relatedDemand }}
						</view>
					</view>
				</view>
				<view class="info">
					<view class="left">
						<view class="time">
							<view>
								<image src="/static/icon04.png"></image>
							</view>
							<view class="text">收货时间{{ item.receivingTime }}</view>
						</view>
						<view class="business">
							<view class="shop">
								<image class="icon" src="/static/icon03.png" mode="widthFix"></image>
								<view class="name">{{ item.shopName }}</view>
							</view>
							<view class="createTime">
								<uni-dateformat :date="new Date(item.createTime)" :threshold="[60000, 3600000]"
									format="yyyy-MM-dd hh"></uni-dateformat>
								·发布
							</view>
						</view>
					</view>
					<view class="right">
						<view class="price">￥{{ item.price }}</view>
						<view class="btn">申请接单</view>
					</view>
				</view>
			</view>
		</view>
		<view class="empty" v-if="list.length <= 0">
			<image src="/static/img32.png" mode="widthFix"></image>
		</view>
	</view>
</template>

<script>
export default {
	data() {
		return {
			query: {
				pageNum: 1,
				pageSize: 10,
				parentId: "",
				queryName: "",
				classId: "",
			},
			total: 0,
			list: [],
		};
	},
	async onReachBottom() {
		if (this.total > this.query.pageNum * this.query.pageSize) {
			this.query.pageNum += 1
			await this.getList()
		} else {
			this.toast('没有更多了')
		}
	},
	onPullDownRefresh() {
		this.getList(true).then(() => {
			uni.stopPullDownRefresh()
		})
	},
	onLoad(options) {
		this.query.queryName = options.queryName || ""
		this.query.classId = options.typeId || ""
		this.getList()
	},
	methods: {
		async getList(reset) {
			if (reset) this.query.pageNum = 1
			let res = await this.request('Order/TaskList', this.query)
			if (this.query.pageNum == 1) {
				this.list = res.data.items
				this.total = res.data.total
			} else {
				this.list.push(...res.data.items)
			}
		},
	}
}
</script>

<style lang="scss" scoped>
.searchBox {
	padding: 32rpx;
	background: white;
}

.list {
	.item {
		margin-bottom: 24rpx;
		background: #FFFFFF;
		border-radius: 16rpx;
		padding: 24rpx;

		.header {
			display: flex;

			image {
				width: 128rpx;
				height: 128rpx;
				border-radius: 8rpx;
			}

			.name {
				flex: 1;
				margin-left: 20rpx;
				font-size: 32rpx;
				color: #333333;

				.text {
					overflow: hidden;
					text-overflow: ellipsis;
					display: -webkit-box;
					-webkit-box-orient: vertical;
					-webkit-line-clamp: 2;
				}
			}
		}

		.info {
			display: flex;
			justify-content: space-between;
			margin-top: 16rpx;

			.left {
				flex: 1;

				.time {
					display: flex;
					align-items: center;
					padding: 14rpx 18rpx;
					background: linear-gradient(270deg, #F5F5F5 70%, rgba(245, 245, 245, 0) 100%);

					image {
						width: 32rpx;
						height: 32rpx;
					}

					.text {
						font-size: 24rpx;
						color: #666666;
						margin-left: 6rpx;
					}
				}

				.business {
					flex: 1;
					display: flex;
					justify-content: space-between;
					align-items: center;
					margin-top: 40rpx;

					.shop {
						display: flex;
						align-items: center;

						.icon {
							width: 44rpx;
							height: 44rpx;
						}

						.name {
							white-space: nowrap;
							margin-left: 6rpx;
							font-size: 24rpx;
							color: #333333;
						}
					}

					.createTime {
						padding-right: 56rpx;
						font-size: 24rpx;
						color: #666666;
					}
				}
			}

			.right {
				display: flex;
				flex-direction: column;
				align-items: flex-end;

				.price {
					font-weight: 600;
					font-size: 40rpx;
					color: #DE4343;
				}

				.btn {
					margin-top: 16rpx;
					width: 192rpx;
					height: 64rpx;
					background: #262626;
					border-radius: 16rpx;
					display: flex;
					align-items: center;
					justify-content: center;
					font-size: 28rpx;
					color: #FFFFFF;
				}
			}
		}
	}
}
</style>